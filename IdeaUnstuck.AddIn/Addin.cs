using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using IdeaUnstuck.Core;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swpublished;
using Environment = System.Environment;

[assembly: ComVisible(false)]
namespace IdeaUnstuck.AddIn
{
    [ComVisible(true), Guid("F46F5028-B3A7-4C13-A604-440A672589B0"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IUnstuckAutomation
    {
        [DispId(1)] string GetStatus();
        [DispId(2)] string GetDiagnostics();
        [DispId(3)] bool ShowPane();
        [DispId(4)] string ExportLog();
    }

    [ComVisible(true), Guid("962CE2F1-BF2C-46F3-A8A5-DC6081767E13"), ProgId("IdeaUnstuck.AddIn"), ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(IUnstuckAutomation))]
    public sealed class Addin : ISwAddin, IUnstuckAutomation
    {
        public const string Id = "{962CE2F1-BF2C-46F3-A8A5-DC6081767E13}";
        readonly Telemetry log = new Telemetry();
        ISldWorks app;
        ITaskpaneView taskpane;
        UnstuckPane pane;
        ApiGuard api;
        Timer timer;
        bool busy, disposed;
        DSldWorksEvents_Event appEvents;
        DPartDocEvents_Event partEvents;
        IModelDoc2 boundPart;
        StateReader reader;
        DiagnosisTracker diagnoses;
        GuidedPaths paths;
        bool connected;
        int documentType, renderCount;
        // Read-only automation endpoint; also supplies the IDispatch required by the host.
        public string GetStatus() => (connected && !disposed ? "connected" : "disconnected") + "|" + documentType + "|" + renderCount + "|" + log.ExceptionCount;
        public string GetDiagnostics() => "Log: " + log.Folder + "; error: " + log.LastWriteError + "; pane visible: " + pane?.Visible + "; child controls: " + pane?.Controls.Count + "; pane: " + pane?.Size;
        public bool ShowPane() => api != null && taskpane != null && api.Get("ITaskpaneView.ShowView", () => taskpane.ShowView());
        public string ExportLog() { log.Write("button_pressed", new { action = "export" }); return log.ExportToday() ?? ""; }
        public bool ConnectToSW(object ThisSW, int Cookie)
        {
            try
            {
                disposed = false;
                log.Write("connect_requested", new { Cookie });
                api = new ApiGuard(log); app = (ISldWorks)ThisSW;
                if (!api.Get("ISldWorks.SetAddinCallbackInfo2", () => app.SetAddinCallbackInfo2(0, this, Cookie))) { log.Write("connect_failed", new { reason = "Callback registration returned false." }); DisconnectFromSW(); return false; }
                log.Write("session_start", new { user = Environment.UserName, solidworks = api.Get("ISldWorks.RevisionNumber", () => app.RevisionNumber(), "unknown"), addin = typeof(Addin).Assembly.GetName().Version.ToString() });
                reader = new StateReader(log); diagnoses = new DiagnosisTracker(log); paths = new GuidedPaths(log);
                taskpane = api.Get("ISldWorks.CreateTaskpaneView2", () => app.CreateTaskpaneView2("", "IDEA Unstuck") as ITaskpaneView);
                if (taskpane == null) { DisconnectFromSW(); return false; }
                pane = api.Get("ITaskpaneView.AddControl", () => taskpane.AddControl("IdeaUnstuck.Pane", "") as UnstuckPane);
                if (pane == null) { log.Write("connect_failed", new { reason = "Task pane control could not be created." }); DisconnectFromSW(); return false; }
                pane.Initialize(log, OnAction);
                api.Get("ITaskpaneView.ShowView", () => taskpane.ShowView());
                timer = new Timer { Interval = 1500 };
                timer.Tick += Tick;
                appEvents = (DSldWorksEvents_Event)app;
                api.Run("subscribe.ActiveDocChangeNotify", () => appEvents.ActiveDocChangeNotify += ActiveChanged);
                api.Run("subscribe.FileNewNotify2", () => appEvents.FileNewNotify2 += FileNew);
                api.Run("subscribe.FileOpenPostNotify", () => appEvents.FileOpenPostNotify += FileOpened);
                RefreshBinding();
                connected = true;
                return true;
            }
            catch (Exception ex) { log.Exception("ConnectToSW", ex); DisconnectFromSW(); return false; }
        }
        public bool DisconnectFromSW()
        {
            try
            {
                if (disposed) return true;
                disposed = true;
                connected = false;
                if (timer != null) { timer.Stop(); timer.Tick -= Tick; timer.Dispose(); }
                DetachPart();
                if (appEvents != null)
                {
                    api.Run("unsubscribe.ActiveDocChangeNotify", () => appEvents.ActiveDocChangeNotify -= ActiveChanged);
                    api.Run("unsubscribe.FileNewNotify2", () => appEvents.FileNewNotify2 -= FileNew);
                    api.Run("unsubscribe.FileOpenPostNotify", () => appEvents.FileOpenPostNotify -= FileOpened);
                }
                diagnoses?.Reset(DateTime.UtcNow);
                if (taskpane != null) api.Get("ITaskpaneView.DeleteView", () => taskpane.DeleteView());
                pane?.Dispose();
                log.Write("session_end", new { user = Environment.UserName, solidworks = api?.Get("ISldWorks.RevisionNumber", () => app.RevisionNumber(), "unknown"), addin = typeof(Addin).Assembly.GetName().Version.ToString() });
                taskpane = null; appEvents = null; app = null;
                return true;
            }
            catch (Exception ex) { log.Exception("DisconnectFromSW", ex); return false; }
        }
        int ActiveChanged() { try { QueueRefresh(); } catch (Exception ex) { log.Exception("ActiveChanged", ex); } return 0; }
        int FileNew(object doc, int type, string template) { try { log.Write("document_created", new { type, template }); QueueRefresh(); } catch (Exception ex) { log.Exception("FileNew", ex); } return 0; }
        int FileOpened(string name) { try { log.Write("document_opened", new { name }); QueueRefresh(); } catch (Exception ex) { log.Exception("FileOpened", ex); } return 0; }
        int FileSaved(int saveType, string name) { try { log.Write("document_saved", new { saveType, name }); } catch (Exception ex) { log.Exception("FileSaved", ex); } return 0; }
        int UndoHappened() { try { diagnoses.Undo(DateTime.UtcNow); } catch (Exception ex) { log.Exception("UndoHappened", ex); } return 0; }
        void QueueRefresh()
        {
            if (disposed || pane == null || pane.IsDisposed) return;
            pane.BeginInvoke((Action)(() => { try { RefreshBinding(); } catch (Exception ex) { log.Exception("deferred_refresh", ex); } }));
        }
        void DetachPart()
        {
            if (partEvents != null)
            {
                api.Run("unsubscribe.UndoPostNotify", () => partEvents.UndoPostNotify -= UndoHappened);
                api.Run("unsubscribe.FileSavePostNotify", () => partEvents.FileSavePostNotify -= FileSaved);
            }
            partEvents = null; boundPart = null;
        }
        void RefreshBinding()
        {
            if (disposed) return;
            timer.Stop();
            var doc = api.Get("ISldWorks.ActiveDoc", () => app.ActiveDoc as IModelDoc2);
            if (!ReferenceEquals(doc, boundPart))
            {
                DetachPart(); diagnoses.Reset(DateTime.UtcNow); paths.Reset();
                if (doc != null && api.Get("IModelDoc2.GetType", () => doc.GetType()) == 1)
                {
                    boundPart = doc;
                    partEvents = api.Get("QueryInterface.DPartDocEvents_Event", () => (DPartDocEvents_Event)doc);
                    if (partEvents != null)
                    {
                        api.Run("subscribe.UndoPostNotify", () => partEvents.UndoPostNotify += UndoHappened);
                        api.Run("subscribe.FileSavePostNotify", () => partEvents.FileSavePostNotify += FileSaved);
                    }
                }
            }
            ReadAndRender();
            if (boundPart != null) timer.Start();
        }
        void Tick(object sender, EventArgs args)
        {
            try
            {
                if (busy || disposed) return;
                var doc = api.Get("ISldWorks.ActiveDoc", () => app.ActiveDoc as IModelDoc2);
                if (doc == null || api.Get("IModelDoc2.GetType", () => doc.GetType()) != 1) { RefreshBinding(); return; }
                ReadAndRender();
            }
            catch (Exception ex) { log.Exception("timer_tick", ex); }
        }
        void ReadAndRender()
        {
            if (busy || disposed) return;
            busy = true;
            try
            {
                var state = reader.Read(app);
                var list = new Rules().Evaluate(state);
                diagnoses.Update(state, list, DateTime.UtcNow); paths.Update(state);
                pane.Render(state, list, paths.Paths);
                documentType = state.DocumentType;
                renderCount++;
            }
            catch (Exception ex) { log.Exception("ReadAndRender", ex); }
            finally { busy = false; }
        }
        void OnAction(string action)
        {
            try
            {
                if (busy || disposed) return;
                if (action == "copy")
                {
                    log.Write("button_pressed", new { action });
                    var state = reader.Read(app);
                    Clipboard.SetText(RescueActions.AiPrompt(state, new Rules().Evaluate(state)));
                    pane.ShowMessage("Now press Windows+Shift+S, drag over the whole SOLIDWORKS window, and paste it into the same chat.");
                }
                else if (action == "export")
                {
                    var file = ExportLog();
                    pane.ShowMessage(string.IsNullOrEmpty(file) ? "The log export did not finish. Ask a TA." : "Today's logs are on your Desktop: " + System.IO.Path.GetFileName(file));
                }
                else { pane.ShowMessage(new RescueActions(log).Run(app, action)); ReadAndRender(); }
            }
            catch (Exception ex) { log.Exception("button_handler", ex); pane?.ShowMessage("That click did not finish. Ask a TA."); }
        }
        [ComRegisterFunction]
        public static void Register(Type type)
        {
            var logger = new Telemetry();
            try
            {
                using (var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = machine.CreateSubKey(@"SOFTWARE\SolidWorks\Addins\" + Id))
                { key.SetValue("", 1, RegistryValueKind.DWord); key.SetValue("Title", "IDEA Unstuck"); key.SetValue("Description", "Plain words and one next click for first-time students."); }
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\SolidWorks\AddInsStartup\" + Id)) key.SetValue("", 1, RegistryValueKind.DWord);
            }
            catch (Exception ex) { logger.Exception("COM_register", ex); throw; }
        }
        [ComUnregisterFunction]
        public static void Unregister(Type type)
        {
            var logger = new Telemetry();
            try
            {
                using (var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) machine.DeleteSubKeyTree(@"SOFTWARE\SolidWorks\Addins\" + Id, false);
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\SolidWorks\AddInsStartup\" + Id, false);
            }
            catch (Exception ex) { logger.Exception("COM_unregister", ex); throw; }
        }
    }
}
