using System;
using System.Linq;
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
        [DispId(5)] string GetMetrics();
        [DispId(6)] void ResetMetrics();
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
        bool busy, disposed, refreshQueued;
        DSldWorksEvents_Event appEvents;
        DPartDocEvents_Event partEvents;
        IModelDoc2 boundPart;
        StateReader reader;
        DiagnosisTracker diagnoses;
        GuidedPaths paths;
        ProgressTelemetry progress;
        bool connected;
        int documentType, renderCount;
        readonly PollStatistics polls = new PollStatistics();
        int blankCards;
        public string GetMetrics() => new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(new { schema = 1, poll = polls.Snapshot(), blankCards, exceptions = log.ExceptionCount, intervalMs = timer?.Interval ?? 1500, connected, build = typeof(Addin).Assembly.ManifestModule.ModuleVersionId.ToString() });
        public void ResetMetrics() { polls.Reset(); blankCards = 0; }
        // Read-only automation endpoint; also supplies the IDispatch required by the host.
        public string GetStatus() => (connected && !disposed ? "connected" : "disconnected") + "|" + documentType + "|" + renderCount + "|" + log.ExceptionCount;
        public string GetDiagnostics() => "Log: " + log.Folder + "; error: " + log.LastWriteError + "; pane visible: " + pane?.Visible + "; child controls: " + pane?.Controls.Count + "; pane: " + pane?.Size + "; help entries: " + pane?.HelpEntryCount;
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
                progress = new ProgressTelemetry(log);
                taskpane = CreateTaskpane();
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
                api.Run("subscribe.CommandOpenPreNotify", () => appEvents.CommandOpenPreNotify += CommandOpened);
                api.Run("subscribe.CommandCloseNotify", () => appEvents.CommandCloseNotify += CommandClosed);
                RefreshBinding();
                connected = true;
                return true;
            }
            catch (Exception ex) { log.Exception("ConnectToSW", ex); DisconnectFromSW(); return false; }
        }
        // Task pane tab icon. CreateTaskpaneView3 (SOLIDWORKS 2017 and later) takes a
        // list of PNG paths, one per size; the sizes are the ones xarial xCAD passes
        // (20, 32, 40, 64, 96, 128). If it fails, fall back to v0.2's call with the
        // 16x18 bitmap, and last to no icon at all, so an icon problem never stops the pane.
        static readonly int[] TaskpaneIconSizes = { 20, 32, 40, 64, 96, 128 };
        ITaskpaneView CreateTaskpane()
        {
            string folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Addin).Assembly.Location) ?? "", "icons");
            var pngs = TaskpaneIconSizes.Select(s => System.IO.Path.Combine(folder, "unstuck-" + s + ".png")).ToArray();
            ITaskpaneView view = null;
            if (pngs.All(System.IO.File.Exists)) view = api.Get("ISldWorks.CreateTaskpaneView3", () => app.CreateTaskpaneView3(pngs, "IDEA Unstuck") as ITaskpaneView);
            string bmp = System.IO.Path.Combine(folder, "taskpane-16x18.bmp");
            if (view == null && System.IO.File.Exists(bmp)) view = api.Get("ISldWorks.CreateTaskpaneView2.icon", () => app.CreateTaskpaneView2(bmp, "IDEA Unstuck") as ITaskpaneView);
            if (view == null) view = api.Get("ISldWorks.CreateTaskpaneView2", () => app.CreateTaskpaneView2("", "IDEA Unstuck") as ITaskpaneView);
            log.Write("taskpane_created", new { icon = view == null ? "none" : pngs.All(System.IO.File.Exists) ? "png-list" : "bitmap-or-none" });
            return view;
        }
        // Esc in the search box: put the keyboard back in the SOLIDWORKS graphics area,
        // so shortcuts work again. Tries the active view window, then the main frame.
        void FocusSolidWorks()
        {
            long hwnd = 0;
            var doc = api.Get("ISldWorks.ActiveDoc", () => app.ActiveDoc as IModelDoc2);
            var view = doc == null ? null : api.Get("IModelDoc2.ActiveView", () => doc.ActiveView as IModelView);
            if (view != null) hwnd = api.Get("IModelView.GetViewHWndx64", () => view.GetViewHWndx64());
            if (hwnd == 0) { var frame = api.Get("ISldWorks.Frame", () => app.Frame() as IFrame); if (frame != null) hwnd = api.Get("IFrame.GetHWndx64", () => frame.GetHWndx64()); }
            if (hwnd != 0) NativeMethods.SetFocus(new IntPtr(hwnd));
        }
        static class NativeMethods { [DllImport("user32.dll")] public static extern IntPtr SetFocus(IntPtr hWnd); }

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
                    api.Run("unsubscribe.CommandOpenPreNotify", () => appEvents.CommandOpenPreNotify -= CommandOpened);
                    api.Run("unsubscribe.CommandCloseNotify", () => appEvents.CommandCloseNotify -= CommandClosed);
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
        int CommandOpened(int command, int userCommand) { try { if (!disposed && boundPart != null) diagnoses.CommandOpened(command); } catch(Exception ex) { log.Exception("CommandOpened",ex); } return 0; }
        int CommandClosed(int command, int reason) { try { if (!disposed && boundPart != null) diagnoses.CommandClosed(command,reason,DateTime.UtcNow); } catch(Exception ex) { log.Exception("CommandClosed",ex); } return 0; }
        void QueueRefresh()
        {
            if (disposed || refreshQueued || pane == null || pane.IsDisposed) return;
            refreshQueued = true;
            try { pane.BeginInvoke((Action)(() => { refreshQueued = false; try { if (!disposed) RefreshBinding(); } catch (Exception ex) { log.Exception("deferred_refresh", ex); } })); }
            catch { refreshQueued = false; throw; }
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
                ReadAndRender(true);
            }
            catch (Exception ex) { log.Exception("timer_tick", ex); }
        }
        void ReadAndRender(bool isPoll = false)
        {
            if (busy || disposed) return;
            if (isPoll && !polls.Enter()) return;
            var watch = System.Diagnostics.Stopwatch.StartNew();
            busy = true;
            try
            {
                var state = reader.Read(app);
                if (disposed || pane == null || pane.IsDisposed) return;
                var list = new Rules().Evaluate(state);
                diagnoses.Update(state, list, DateTime.UtcNow); paths.Update(state);
                progress.Observe(state);
                foreach (var card in list) if (string.IsNullOrWhiteSpace(card.Title) || string.IsNullOrWhiteSpace(card.Meaning) || string.IsNullOrWhiteSpace(card.NextClick)) blankCards++;
                pane.Render(state, list, paths.Paths);
                documentType = state.DocumentType;
                renderCount++;
            }
            catch (Exception ex) { log.Exception("ReadAndRender", ex); }
            finally { busy = false; if (isPoll) polls.Leave(watch.Elapsed.TotalMilliseconds); }
        }
        void OnAction(string action)
        {
            try
            {
                if (busy || disposed) return;
                if (action == "focus-sw") { FocusSolidWorks(); return; }
                if (action == "copy" || action.StartsWith("copy:", StringComparison.Ordinal))
                {
                    var topic = action.Length > 5 ? KnowledgeBase.Embedded.Find(action.Substring(5)) : null;
                    log.Write("button_pressed", new { action = "copy", topic = topic?.Id });
                    var state = reader.Read(app);
                    var text = RescueActions.AiPrompt(state, new Rules().Evaluate(state));
                    if (topic != null) text += "\r\nI already read the class help \"" + topic.Title + "\" and it did not fix it.";
                    Clipboard.SetText(text);
                    pane.ShowMessage("Copied. If you want AI help, paste it into your chat. Then press Windows+Shift+S, drag over the whole SOLIDWORKS window, and paste the picture too.");
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
                { key.SetValue("", 1, RegistryValueKind.DWord); key.SetValue("Title", "IDEA Unstuck"); key.SetValue("Description", "Plain-word help for students new to SOLIDWORKS. Works offline.");
                  // Add-Ins list icon. Not confirmed in the SOLIDWORKS API Help (its pages would not load here); harmless if ignored.
                  var icon = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(type.Assembly.Location) ?? "", "icons", "addin-16.bmp");
                  if (System.IO.File.Exists(icon)) key.SetValue("Icon Path", icon); }
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
