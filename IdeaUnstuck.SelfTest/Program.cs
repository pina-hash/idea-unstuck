using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IdeaUnstuck.Core;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using Environment = System.Environment;

namespace IdeaUnstuck.SelfTest
{
    static class Program
    {
        static int passed, failed;
        static string Filter = "";
        static bool KeepOpen;
        static readonly Telemetry Log = new Telemetry();
        static readonly ApiGuard Api = new ApiGuard(Log);
        static readonly StateReader Reader = new StateReader(Log);
        static readonly RescueActions Rescue = new RescueActions(Log);
        static ISldWorks app;
        static IModelDoc2 doc;
        static readonly List<string> owned = new List<string>();
        static string partTemplate, assemblyTemplate;
        static string Output = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-results");
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                Directory.CreateDirectory(Output);
                Filter = args.FirstOrDefault(a => a.StartsWith("--filter="))?.Substring(9) ?? "";
                KeepOpen = args.Contains("--inspect");
                if (args.Contains("--diagnose"))
                {
                    var host = Api.Get("COM.GetActiveObject", () => Marshal.GetActiveObject("SldWorks.Application") as ISldWorks);
                    object instance = host == null ? null : Api.Get("ISldWorks.GetAddInObject", () => host.GetAddInObject("IdeaUnstuck.AddIn"));
                    Console.WriteLine("Object="+instance?.GetType().FullName+"; COM="+(instance != null && Marshal.IsComObject(instance)));
                    Console.WriteLine("Proxy="+System.Runtime.Remoting.RemotingServices.IsTransparentProxy(instance));
                    if (instance != null) Console.WriteLine((string)((dynamic)instance).GetDiagnostics());
                    if (instance != null) Console.WriteLine("Status="+(string)((dynamic)instance).GetStatus());
                    if (instance != null && args.Contains("--show-pane")) Console.WriteLine("ShowPane="+(bool)((dynamic)instance).ShowPane());
                    if (instance != null && args.Contains("--export")) Console.WriteLine("Export="+(string)((dynamic)instance).ExportLog());
                    return instance != null && Log.ExceptionCount == 0 ? 0 : 1;
                }
                if (args.Contains("--pane-preview"))
                {
                    Application.EnableVisualStyles();
                    var preview = new IdeaUnstuck.AddIn.UnstuckPane(Log, id => Console.WriteLine("Preview button: "+id)) { Dock=DockStyle.Fill };
                    var sample = new StateSnapshot { DocumentType=1, FileName="SMITH_FIRST_PART.SLDPRT", DocumentKey="preview", LengthUnit=0, BodyCount=0, InSketch=true, Sketch=new SketchState { Name="Sketch1", Support="Front Plane", EntityCount=3, OpenContours=1, ContourCount=1, ConstraintCode=2 } };
                    var paths=new GuidedPaths(Log); paths.Update(sample); preview.Render(sample,new Rules().Evaluate(sample),paths.Paths);
                    using(var form=new Form { Text="IDEA Unstuck pane layout preview", Width=340, Height=940, StartPosition=FormStartPosition.CenterScreen }) { form.Controls.Add(preview); Application.Run(form); }
                    return 0;
                }
                if (args.Contains("--shutdown-if-empty"))
                {
                    var running = Api.Get("COM.GetActiveObject", () => Marshal.GetActiveObject("SldWorks.Application") as ISldWorks);
                    int count = running == null ? -1 : Api.Get("ISldWorks.GetDocumentCount", () => running.GetDocumentCount(), -1);
                    Console.WriteLine("Open documents="+count);
                    if (count != 0) return 1;
                    bool exited = Api.Run("ISldWorks.ExitApp", () => running.ExitApp());
                    Api.Get("COM.FinalReleaseComObject", () => Marshal.FinalReleaseComObject(running));
                    return exited ? 0 : 1;
                }
                UnitCases();
                if (!args.Contains("--unit"))
                {
                    app = args.Contains("--startup")
                        ? Api.Get("COM.GetActiveObject", () => Marshal.GetActiveObject("SldWorks.Application") as ISldWorks)
                        : Api.Get("COM.SldWorks.Application", () => (ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application", true)));
                    if (app == null) { Case("SOLIDWORKS starts through COM", () => Require(false, "COM activation failed. See telemetry.")); return Finish(); }
                    Api.Run("ISldWorks.Visible", () => app.Visible = true);
                    string revision = Api.Get("ISldWorks.RevisionNumber", () => app.RevisionNumber(), "unknown");
                    Console.WriteLine("SOLIDWORKS revision " + revision);
                    string year = (1992 + int.Parse(revision.Split('.')[0])).ToString();
                    partTemplate = Api.Get("ISldWorks.GetUserPreferenceStringValue", () => app.GetUserPreferenceStringValue((int)swUserPreferenceStringValue_e.swDefaultTemplatePart));
                    assemblyTemplate = Api.Get("ISldWorks.GetUserPreferenceStringValue", () => app.GetUserPreferenceStringValue((int)swUserPreferenceStringValue_e.swDefaultTemplateAssembly));
                    if (!File.Exists(partTemplate)) partTemplate = @"C:\ProgramData\SOLIDWORKS\SOLIDWORKS " + year + @"\templates\Part.PRTDOT";
                    if (!File.Exists(assemblyTemplate)) assemblyTemplate = @"C:\ProgramData\SOLIDWORKS\SOLIDWORKS " + year + @"\templates\Assembly.ASMDOT";
                    Require(File.Exists(partTemplate), "Part template missing: " + partTemplate);
                    if (args.Contains("--addin")) AddinCases(args.Contains("--startup")); else LiveCases();
                }
            }
            catch (Exception ex) { Log.Exception("SelfTest.Main", ex); failed++; Console.WriteLine("FAIL runner: " + ex.Message); }
            finally
            {
                if (KeepOpen && app != null) {
                    Console.WriteLine("Inspection window is open. Waiting up to 5 minutes or for inspect.stop.");
                    var until=DateTime.UtcNow.AddMinutes(5);
                    while(DateTime.UtcNow<until && !File.Exists(System.IO.Path.Combine(Output,"inspect.stop"))) { Application.DoEvents(); System.Threading.Thread.Sleep(100); }
                }
                if (app != null)
                {
                    foreach (var name in owned) Api.Run("ISldWorks.CloseDoc", () => app.CloseDoc(name));
                    // Leave the application open for inspection; never close a student's running application.
                }
            }
            return Finish();
        }
        static int Finish() { Console.WriteLine("TOTAL PASS=" + passed + " FAIL=" + failed); return failed == 0 ? 0 : 1; }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
        static void Case(string name, Action test)
        {
            if (Filter.Length > 0 && name.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) < 0) return;
            int errors = Log.ExceptionCount;
            try { test(); Require(Log.ExceptionCount == errors, "An API exception was caught. See telemetry."); passed++; Console.WriteLine("PASS " + name); }
            catch (Exception ex) { failed++; Console.WriteLine("FAIL " + name + ": " + ex.Message); Log.Write("selftest_failure", new { name, ex.Message }); }
            Application.DoEvents();
        }
        static StateSnapshot Read()
        {
            var state = Reader.Read(app);
            File.WriteAllText(System.IO.Path.Combine(Output, "last-snapshot.json"), new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(state));
            Require(state.ReadIssues.Count == 0, "State read issues: " + string.Join(", ", state.ReadIssues));
            return state;
        }
        static bool Has(string id) => new Rules().Evaluate(Read()).Any(d => d.Id == id || d.Id.StartsWith(id + ":", StringComparison.Ordinal));
        static void NewPart()
        {
            if (doc != null)
            {
                string title = Api.Get("IModelDoc2.GetTitle", () => doc.GetTitle());
                Api.Run("ISldWorks.CloseDoc", () => app.CloseDoc(title)); owned.Remove(title); doc = null;
            }
            doc = Api.Get("ISldWorks.NewDocument", () => app.NewDocument(partTemplate, 0, 0, 0) as IModelDoc2);
            Require(doc != null, "NewDocument returned null.");
            Api.Run("IModelDoc2.ShowFeatureErrorDialog", () => doc.ShowFeatureErrorDialog = false);
            owned.Add(Api.Get("IModelDoc2.GetTitle", () => doc.GetTitle()));
        }
        static ISketchManager Manager() => Api.Get("IModelDoc2.SketchManager", () => doc.SketchManager as ISketchManager);
        static void Rect(double x = 0, double y = 0, double size = .0254)
        {
            var sm = Manager();
            Require(Api.Get("ISketchManager.CreateCornerRectangle", () => sm.CreateCornerRectangle(x, y, 0, x + size, y + size, 0)) != null, "Rectangle creation failed.");
        }
        static void Begin() { string result = Rescue.Run(app, "front"); Require(Read().InSketch, result); }
        static IFeature Extrude(bool merge = true)
        {
            Rescue.Run(app, "exit");
            var state = Read();
            var sk = Api.Get("IPartDoc.FeatureByName", () => ((IPartDoc)doc).FeatureByName(state.LastSketch.Name) as IFeature);
            Api.Run("IModelDoc2.ClearSelection2", () => doc.ClearSelection2(true));
            Require(Api.Get("IFeature.Select2", () => sk.Select2(false, 0)), "Select sketch failed.");
            var fm = Api.Get("IModelDoc2.FeatureManager", () => doc.FeatureManager);
            var f = Api.Get("IFeatureManager.FeatureExtrusion2", () => fm.FeatureExtrusion2(true, false, false, (int)swEndConditions_e.swEndCondMidPlane, 0, .0127, .0127, false, false, false, false, 0, 0, false, false, false, false, merge, true, true, 0, 0, false) as IFeature);
            Require(f != null, "Extrude returned null."); return f;
        }
        static void LiveCases()
        {
            Case("millimeters diagnosis and inches/3-decimal rescue", () => {
                NewPart(); Api.Get("IModelDoc2.SetUserPreferenceIntegerValue", () => doc.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swUnitSystem, (int)swUnitSystem_e.swUnitSystem_MMGS));
                Require(Has("millimeters"), "Missing millimeters diagnosis."); Rescue.Run(app, "units"); var s = Read();
                Require(s.UnitSystem == 3 && s.LengthUnit == 3 && s.DecimalPlaces == 3, "Units or precision did not change.");
            });
            Case("start on Front Plane, exit, left-sketch diagnosis, re-enter", () => {
                NewPart(); Begin(); Require(Read().Sketch.Support == "Front Plane", "Wrong sketch support."); Rect(); Rescue.Run(app, "exit");
                Require(!Read().InSketch && Has("left-sketch"), "Missing left-sketch diagnosis."); Rescue.Run(app, "edit"); Require(Read().InSketch, "Edit Sketch failed.");
            });
            Case("open contour diagnosis", () => {
                NewPart(); Begin(); var sm = Manager(); Api.Get("ISketchManager.CreateLine", () => sm.CreateLine(0,0,0,.0254,0,0));
                Require(Has("open-contour"), "Open line was not detected.");
            });
            Case("two side-by-side outlines diagnosis", () => { NewPart(); Begin(); Rect(); Rect(.05); Require(Has("multiple-outlines"), "Two outlines not detected."); });
            Case("crossing sketch reports native contour error without changing selection", () => {
                NewPart(); Begin(); var sm=Manager();
                Api.Get("ISketchManager.CreateLine",()=>sm.CreateLine(0,0,0,.0254,.0254,0));
                Api.Get("ISketchManager.CreateLine",()=>sm.CreateLine(.0254,.0254,0,0,.0254,0));
                Api.Get("ISketchManager.CreateLine",()=>sm.CreateLine(0,.0254,0,.0254,0,0));
                Api.Get("ISketchManager.CreateLine",()=>sm.CreateLine(.0254,0,0,0,0,0));
                var sel=Api.Get("IModelDoc2.SelectionManager",()=>doc.SelectionManager as ISelectionMgr);
                int count=Api.Get("ISelectionMgr.GetSelectedObjectCount2",()=>sel.GetSelectedObjectCount2(-1));
                var state=Read();Require(state.Sketch.UsageCode!=0,"Crossing outline not reported by native validation.");
                Require(new Rules().Evaluate(state).Any(d=>d.Id.StartsWith("sketch-error:")),"Missing crossing-sketch diagnosis.");
                Require(Api.Get("ISelectionMgr.GetSelectedObjectCount2",()=>sel.GetSelectedObjectCount2(-1))==count,"Validation changed selection.");
            });
            Case("over-defined sketch diagnosis and Undo", () => {
                NewPart(); Begin(); var sm = Manager();
                var line = Api.Get("ISketchManager.CreateLine", () => sm.CreateLine(0,0,0,.0254,0,0) as ISketchSegment);
                Api.Get("ISketchSegment.Select4", () => line.Select4(false, null));
                Api.Run("IModelDoc2.SketchAddConstraints", () => doc.SketchAddConstraints("sgFIXED"));
                Api.Get("ISketchSegment.Select4", () => line.Select4(false, null));
                Api.Run("IModelDoc2.SketchAddConstraints", () => doc.SketchAddConstraints("sgVERTICAL"));
                Require(Has("over-defined"), "Fixture did not produce an over-defined sketch. Status=" + Read().Sketch.ConstraintName);
                Rescue.Run(app, "undo"); Require(!Has("over-defined"), "Undo did not clear conflicting relation.");
            });
            Case("failed fillet diagnosis, large radius, Undo", () => {
                NewPart(); Begin(); Rect(); var boss = Extrude();
                var bodies = Api.Get("IPartDoc.GetBodies2", () => ((IPartDoc)doc).GetBodies2(0, false) as object[]);
                var edges = Api.Get("IBody2.GetEdges", () => ((IBody2)bodies[0]).GetEdges() as object[]);
                Api.Run("IModelDoc2.ClearSelection2", () => doc.ClearSelection2(true));
                var selections=Api.Get("IModelDoc2.SelectionManager",()=>doc.SelectionManager as ISelectionMgr);
                var selectionData=Api.Get("ISelectionMgr.CreateSelectData",()=>selections.CreateSelectData());
                Api.Run("ISelectData.Mark",()=>selectionData.Mark=1);
                Require(Api.Get("IEntity.Select4", () => ((IEntity)edges[0]).Select4(false, selectionData)),"Could not select the fillet edge.");
                foreach(var edge in edges.Skip(1)) Require(Api.Get("IEntity.Select4",()=>((IEntity)edge).Select4(true,selectionData)),"Could not select a fillet edge.");
                var fm = Api.Get("IModelDoc2.FeatureManager", () => doc.FeatureManager);
                bool previous = Api.Get("ISldWorks.AllowFailedFeatureCreation", () => app.AllowFailedFeatureCreation(true));
                try
                {
                    var definition=Api.Get("IFeatureManager.CreateDefinition",()=>fm.CreateDefinition((int)swFeatureNameID_e.swFmFillet) as ISimpleFilletFeatureData2);
                    Require(definition!=null,"Fillet definition was not created.");
                    Require(Api.Get("ISimpleFilletFeatureData2.Initialize",()=>definition.Initialize((int)swSimpleFilletType_e.swConstRadiusFillet)),"Fillet definition could not initialize.");
                    Api.Run("ISimpleFilletFeatureData2.ConicTypeForCrossSectionProfile",()=>definition.ConicTypeForCrossSectionProfile=(int)swFeatureFilletProfileType_e.swFeatureFilletCircular);
                    Api.Run("ISimpleFilletFeatureData2.Edges",()=>definition.Edges=edges);
                    Api.Run("ISimpleFilletFeatureData2.IsMultipleRadius",()=>definition.IsMultipleRadius=false);
                    Api.Run("ISimpleFilletFeatureData2.DefaultRadius",()=>definition.DefaultRadius=.001);
                    var fillet = Api.Get("IFeatureManager.CreateFeature", () => fm.CreateFeature(definition) as IFeature);
                    Require(fillet != null, "Failed fillet fixture was not retained.");
                    var debug=Api.Get("IFeature.GetDefinition",()=>fillet.GetDefinition() as ISimpleFilletFeatureData2);
                    Console.WriteLine("Default radius="+Api.Get("ISimpleFilletFeatureData2.DefaultRadius",()=>debug.DefaultRadius));
                    Console.WriteLine("Edges="+Api.Get("ISimpleFilletFeatureData2.GetEdgeCount",()=>debug.GetEdgeCount()));
                    var depthDisplay=Api.Get("IFeature.GetFirstDisplayDimension",()=>boss.GetFirstDisplayDimension() as IDisplayDimension);
                    var depth=Api.Get("IDisplayDimension.GetDimension2",()=>depthDisplay.GetDimension2(0) as IDimension);
                    Api.Run("IDimension.SystemValue",()=>depth.SystemValue=.0001);
                    Api.Get("IModelDoc2.ForceRebuild3", () => doc.ForceRebuild3(false));
                }
                finally { Api.Get("ISldWorks.AllowFailedFeatureCreation", () => app.AllowFailedFeatureCreation(previous)); }
                var filletState=Read();
                Console.WriteLine("Feature errors: "+string.Join("; ",filletState.Features.Where(f=>f.Kind=="Fillet").Select(f=>f.Name+"="+f.ErrorCode+", suppressed="+f.Suppressed+", rolled="+f.RolledBack)));
                Require(Has("feature"), "The large-radius fixture did not report a feature error.");
                Rescue.Run(app, "undo"); Require(!Has("feature"), "Undo did not clear feature error.");
            });
            Case("rollback diagnosis and roll-to-end rescue", () => {
                NewPart(); Begin(); Rect(); var f = Extrude();
                string name = Api.Get("IFeature.Name", () => f.Name);
                var fm = Api.Get("IModelDoc2.FeatureManager", () => doc.FeatureManager);
                Require(Api.Get("IFeatureManager.EditRollback", () => fm.EditRollback(3, name)), "Rollback failed.");
                Require(Has("rollback"), "Missing rollback diagnosis."); Rescue.Run(app, "roll"); Require(!Read().Rollback && Read().BodyCount == 1, "Roll to end failed.");
            });
            Case("sketch without body diagnosis", () => { NewPart(); Begin(); Rect(); Rescue.Run(app, "exit"); Require(Has("no-body"), "Missing no-body diagnosis."); });
            Case("never saved diagnosis and saved-state detection", () => {
                NewPart(); Require(Has("unsaved"), "Missing unsaved diagnosis."); Begin(); Rect(); Rescue.Run(app, "exit");
                var ext = Api.Get("IModelDoc2.Extension", () => doc.Extension);
                int errors = 0, warnings = 0;
                string file = System.IO.Path.Combine(Output, "UNSTUCK_SELFTEST_" + Guid.NewGuid().ToString("N") + ".SLDPRT");
                Require(Api.Get("IModelDocExtension.SaveAs", () => ext.SaveAs(file, 0, 1, null, ref errors, ref warnings)), "Silent save failed: " + errors);
                owned.Add(Api.Get("IModelDoc2.GetTitle", () => doc.GetTitle()));
                Require(Read().Saved && !Has("unsaved"), "Saved state not detected.");
            });
            Case("assembly diagnosis and rescue refusal", () => {
                Require(File.Exists(assemblyTemplate), "Assembly template missing.");
                var asm = Api.Get("ISldWorks.NewDocument", () => app.NewDocument(assemblyTemplate, 0, 0, 0) as IModelDoc2);
                Require(asm != null, "Assembly creation failed.");
                string name = Api.Get("IModelDoc2.GetTitle", () => asm.GetTitle()); owned.Add(name);
                Require(Has("wrong-document"), "Missing assembly diagnosis."); Require(Rescue.Run(app, "units").StartsWith("Open a part"), "Rescue touched assembly.");
                Api.Run("ISldWorks.CloseDoc", () => app.CloseDoc(name)); owned.Remove(name);
            });
            Case("two solid bodies diagnosis", () => { NewPart(); Begin(); Rect(); Rect(.05); Extrude(false); Require(Read().BodyCount == 2 && Has("multiple-bodies"), "Expected two bodies."); });
            Case("zoomed-away part and Find my part rescue", () => {
                NewPart(); Begin(); Rect(); Extrude(); var view = Api.Get("IModelDoc2.ActiveView", () => doc.ActiveView as IModelView);
                Api.Run("IModelView.Scale2", () => view.Scale2 = .000001);
                Rescue.Run(app, "find"); Require(Api.Get("IModelView.Scale2", () => view.Scale2) > .000001, "Zoom to fit did not restore scale.");
                Require(Read().BodyCount == 1, "View rescue changed geometry.");
            });
            Case("state reading leaves document contents unchanged", () => {
                var before = Read(); for (int i = 0; i < 5; i++) Read(); var after = Read();
                Require(before.BodySignature == after.BodySignature && before.Features.Count == after.Features.Count && before.InSketch == after.InSketch, "Read changed part state.");
            });
            Case("Copy for AI contains version, state, causes, and screenshot instruction", () => {
                var s = Read(); var text = RescueActions.AiPrompt(s, new Rules().Evaluate(s));
                Require(text.Contains("SOLIDWORKS 2026") || text.Contains("SOLIDWORKS 2025"), "Missing supported year."); Require(text.Contains("CAUSING") && text.Contains("screenshot"), "Missing prompt instructions.");
            });
        }
        static void UnitCases()
        {
            Case("help: it wont let me extrud returns open-contour first", () => {
                Require(KnowledgeBase.Embedded.Search("it wont let me extrud").FirstOrDefault()?.Entry.Id == "open-contour", "Wrong first extrusion result.");
            });
            Case("help: buttons dissapeared returns toolbar-gone first", () => {
                Require(KnowledgeBase.Embedded.Search("buttons dissapeared").FirstOrDefault()?.Entry.Id == "toolbar-gone", "Wrong first toolbar result.");
            });
            Case("help: every related id resolves", () => {
                var kb = KnowledgeBase.Embedded;
                Require(kb.Entries.Count == 49, "Expected all 49 supplied entries.");
                int count = 0;
                foreach (var entry in kb.Entries) foreach (var id in entry.Related) { Require(kb.Find(id) != null, entry.Id + " links to missing " + id); count++; }
                Console.WriteLine("Help entries=" + kb.Entries.Count + "; related links=" + count);
            });
            Case("help: every diagnosis link resolves to its expected entry", () => {
                var kb = KnowledgeBase.Embedded;
                var expected = new[] { "open-contour|open-contour", "multiple-outlines|multi-contour", "sketch-error:7|multi-contour", "left-sketch|left-sketch", "millimeters|units-mm", "over-defined|red-sketch", "feature:Fillet1:18|red-x", "rollback|rollback", "no-body|extrude", "multiple-bodies|two-bodies", "unsaved|save", "lost-part|lost-part" };
                foreach (var pair in expected) { var fields = pair.Split('|'); Require(kb.ForDiagnosis(fields[0])?.Id == fields[1], "Wrong help for " + fields[0]); }
                foreach (var pair in KnowledgeBase.DiagnosisLinks) Require(kb.Find(pair.Value) != null, "Missing diagnosis target " + pair.Value);
                Require(kb.ForDiagnosis("read-incomplete") == null && kb.ForDiagnosis(null) == null, "Unmapped diagnoses must not invent help.");
                Console.WriteLine("Diagnosis links=" + expected.Length);
            });
            Case("help: exact prefix phrase and all four single-typo scores", () => {
                var kb = KnowledgeBase.Embedded;
                double Score(string query) => kb.Search(query).Single(r => r.Entry.Id == "open-contour").Score;
                Require(Score("open contour") == 10, "Exact words plus whole phrase should score 10.");
                Require(Score("extrud") == 2, "Prefix should score 2.");
                foreach (var query in new[] { "opne", "closd", "clozed", "cloosed" }) Require(Score(query) == 1.5, "Single typo should score 1.5: " + query);
                Require(Score("opne contoru") == 3, "Distinct typo words should accumulate.");
                Require(kb.Search("oen").Count == 0, "Three-letter typo must not match.");
            });
            Case("help: OR matching normalization top five and blank queries", () => {
                var kb = KnowledgeBase.Embedded;
                Require(kb.Search("  ").Count == 0 && kb.Search("zzzzzzzzzz").Count == 0, "Empty or unknown query matched.");
                Require(kb.Search("zzzzzzzzzz opne").First().Entry.Id == "open-contour", "OR matching failed.");
                Require(kb.Search("IT WON'T LET ME EXTRUD!").First().Entry.Id == "open-contour", "Normalization failed.");
                Require(kb.Search("sketch").Count == 5, "Expected five results.");
                Require(kb.Search("opne opne").First().Score == kb.Search("opne").First().Score, "Repeated word inflated score.");
                Require(kb.Search("open cont").First(r => r.Entry.Id == "open-contour").Score == 5, "Partial phrase received a whole-phrase bonus.");
            });
            Case("stuck after three unchanged minutes, then clears", () => {
                var tracker = new DiagnosisTracker(Log); var s = new StateSnapshot { DocumentType = 1, DocumentKey = "test", LengthUnit = 0 };
                var time = new DateTime(2026, 1, 1); var ds = new Rules().Evaluate(s); tracker.Update(s, ds, time); Require(ds.All(d => d.Id != "stuck"), "Triggered too soon.");
                ds = new Rules().Evaluate(s); tracker.Update(s, ds, time.AddSeconds(180)); Require(ds.Any(d => d.Id == "stuck"), "No stuck trigger.");
                s.LengthUnit = 3; ds = new Rules().Evaluate(s); tracker.Update(s, ds, time.AddSeconds(181)); Require(ds.All(d => d.Id != "stuck"), "Did not clear.");
            });
            Case("five undos in one minute, then expires", () => {
                var tracker = new DiagnosisTracker(Log); var s = new StateSnapshot { DocumentType = 1, DocumentKey = "undo" }; var now = DateTime.UtcNow;
                tracker.Update(s, new List<Diagnosis>(), now); for (int i = 0; i < 5; i++) tracker.Undo(now.AddSeconds(i));
                var ds = new List<Diagnosis>(); tracker.Update(s, ds, now.AddSeconds(5)); Require(ds.Any(d => d.Id == "stuck"), "No undo trigger.");
                ds = new List<Diagnosis>(); tracker.Update(s, ds, now.AddSeconds(65)); Require(ds.Count == 0, "Undo trigger did not expire.");
            });
            Case("unknown feature and constraint codes retain raw values", () => { Require(ErrorWords.Feature(98765).Contains("98765") && ErrorWords.Feature(98765).Contains("Ask a TA"), "Unknown feature blank."); Require(ErrorWords.Constraint(98765).Contains("98765"), "Unknown constraint blank."); });
            Case("all installed feature and contour enum codes have words", () => { foreach (int value in Enum.GetValues(typeof(swFeatureError_e))) Require(!string.IsNullOrWhiteSpace(ErrorWords.Feature(value)), "Missing feature words."); foreach (int value in Enum.GetValues(typeof(swSketchCheckFeatureStatus_e))) Require(!string.IsNullOrWhiteSpace(ErrorWords.Contour(value)), "Missing contour words."); });
            Case("path does not complete from a body alone", () => { var p = new GuidedPaths(Log); p.Update(new StateSnapshot { DocumentType = 1, DocumentKey = "path", BodyCount = 1 }); Require(!p.Paths[0].Steps.Last().Done, "First path completed without evidence."); });
            Case("document switch resets stuck detector", () => { var t = new DiagnosisTracker(Log); var s = new StateSnapshot { DocumentType = 1, DocumentKey = "one", LengthUnit = 0 }; var now = DateTime.UtcNow; t.Update(s,new Rules().Evaluate(s),now); s.DocumentKey="two"; var ds=new Rules().Evaluate(s); t.Update(s,ds,now.AddMinutes(4)); Require(ds.All(d=>d.Id!="stuck"),"Stuck leaked between parts."); });
            Case("first-part path follows observed steps and survives Save As", () => {
                var p=new GuidedPaths(Log); var s=new StateSnapshot {DocumentType=1,DocumentKey="stable",FileName="Part1",BodyCount=0,UnitSystem=3,LengthUnit=3}; p.Update(s);
                s.InSketch=true; s.Sketch=new SketchState {Name="Sketch1",Support="Front Plane",ClosedContours=1}; p.Update(s);
                s.Sketch.DimensionCount=1;p.Update(s);s.LastSketch=s.Sketch;s.Sketch=null;s.InSketch=false;p.Update(s);
                s.BodyCount=1;s.Features.Add(new FeatureState{Name="Boss-Extrude1",Kind="Extrusion",MidPlane=true});p.Update(s);
                s.Path="C:\\Class\\SMITH.SLDPRT";s.FileName="SMITH.SLDPRT";p.Update(s);
                Require(p.Paths[0].Steps.All(step=>step.Done),"First-part path lost completion evidence.");
            });
            Case("change-size path tracks edit rollback and a rebuilt body",()=>{
                var p=new GuidedPaths(Log);var s=new StateSnapshot{DocumentType=1,DocumentKey="change",BodyCount=1,BodySignature="before"};p.Update(s);
                s.InSketch=true;s.BodyCount=0;s.Rollback=true;s.Sketch=new SketchState{Name="Sketch1",DimensionCount=1,DimensionSignature="D1=.01"};p.Update(s);
                s.Sketch.DimensionSignature="D1=.02";p.Update(s);s.InSketch=false;s.BodyCount=1;s.BodySignature="after";s.Rollback=false;s.Sketch=null;p.Update(s);
                Require(p.Paths[1].Steps.All(step=>step.Done),"Edit rollback prevented change-size evidence.");
            });
            Case("face-cut path requires a newly observed cut",()=>{
                var p=new GuidedPaths(Log);var s=new StateSnapshot{DocumentType=1,DocumentKey="cut",BodyCount=1,InSketch=true,Sketch=new SketchState{SupportKind="face",ClosedContours=1}};
                s.Features.Add(new FeatureState{Name="Cut1",Kind="Cut"});p.Update(s);s.InSketch=false;s.Sketch=null;p.Update(s);
                Require(!p.Paths[2].Steps[2].Done,"An old cut completed the new cut step.");s.Features.Add(new FeatureState{Name="Cut2",Kind="Cut"});p.Update(s);
                Require(p.Paths[2].Steps.All(step=>step.Done),"New cut was not detected.");
            });
            Case("fillet path does not tick failed features",()=>{
                var p=new GuidedPaths(Log);var s=new StateSnapshot{DocumentType=1,DocumentKey="round",BodyCount=1};var f=new FeatureState{Name="Fillet1",Kind="Fillet",ErrorCode=18};s.Features.Add(f);p.Update(s);
                Require(!p.Paths[3].Steps[1].Done,"Failed fillet was marked successful.");f.ErrorCode=0;p.Update(s);Require(p.Paths[3].Steps.All(step=>step.Done),"Repaired fillet not observed.");
            });
            Case("incomplete reads do not invent document diagnoses",()=>{
                var s=new StateSnapshot{DocumentType=1,BodyCount=0,LengthUnit=0};s.ReadIssues.Add("API unavailable");var ds=new Rules().Evaluate(s);Require(ds.Count==1 && ds[0].Id=="read-incomplete","Unknown data produced misleading diagnoses.");
            });
        }
        static void AddinCases(bool startup)
        {
            string dll = @"C:\Program Files\IDEA\Unstuck\IdeaUnstuck.AddIn.dll";
            dynamic addin = null;
            Case(startup ? "installed add-in loads automatically at startup" : "installed add-in loads through SOLIDWORKS", () => {
                if (!startup) {
                    int result = Api.Get("ISldWorks.LoadAddIn", () => app.LoadAddIn(dll), -1);
                    Console.WriteLine("LoadAddIn="+result); Require(result == 0 || result == 2, "LoadAddIn error " + result);
                }
                addin = Api.Get("ISldWorks.GetAddInObject", () => app.GetAddInObject("IdeaUnstuck.AddIn"));
                Require(addin != null, "Add-in object missing.");
                string status = addin.GetStatus(); Console.WriteLine("Add-in status="+status);
                Console.WriteLine((string)addin.GetDiagnostics());
                Require(status.StartsWith("connected|"), "Add-in did not connect.");
            });
            Case("installed pane renders a new part and polls without exceptions", () => {
                Require(addin != null, "Add-in unavailable.");
                string[] before = ((string)addin.GetStatus()).Split('|');
                NewPart(); Begin(); Rect(); Rescue.Run(app,"exit");
                for(int i=0;i<25;i++){Application.DoEvents();System.Threading.Thread.Sleep(100);}
                string status = addin.GetStatus(); Console.WriteLine("Add-in part status="+status);
                string[] after = status.Split('|');
                Require(after[0]=="connected" && after[1]=="1" && int.Parse(after[2])>int.Parse(before[2]), "Pane did not render the new part.");
                Require(after[3]=="0", "An exception occurred in the installed add-in.");
                Require(Read().BodyCount==0,"Unexpected state.");
                Require((bool)addin.ShowPane(), "The pane could not be shown.");
            });
            Case("installed pane loads all embedded help entries", () => {
                Require(addin != null, "Add-in unavailable.");
                Require(((string)addin.GetDiagnostics()).Contains("; help entries: 49"), "Installed pane did not load the embedded knowledge base.");
            });
            Case("installed add-in exports its teaching log", () => {
                Require(addin != null, "Add-in unavailable.");
                string path = addin.ExportLog();
                Require(!string.IsNullOrEmpty(path) && File.Exists(path), "Teacher export did not produce a ZIP.");
                Console.WriteLine("Teacher export="+path);
            });
        }
    }
}
