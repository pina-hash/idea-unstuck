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
                UnitCases();
                if (!args.Contains("--unit"))
                {
                    app = Api.Get("COM.SldWorks.Application", () => (ISldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application", true)));
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
                    if (args.Contains("--addin")) AddinCases(); else LiveCases();
                }
            }
            catch (Exception ex) { Log.Exception("SelfTest.Main", ex); failed++; Console.WriteLine("FAIL runner: " + ex.Message); }
            finally
            {
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
                NewPart(); Begin(); Rect(); Extrude();
                var bodies = Api.Get("IPartDoc.GetBodies2", () => ((IPartDoc)doc).GetBodies2(0, false) as object[]);
                var edges = Api.Get("IBody2.GetEdges", () => ((IBody2)bodies[0]).GetEdges() as object[]);
                Api.Run("IModelDoc2.ClearSelection2", () => doc.ClearSelection2(true));
                Api.Get("IEntity.Select4", () => ((IEntity)edges[0]).Select4(false, null));
                var fm = Api.Get("IModelDoc2.FeatureManager", () => doc.FeatureManager);
                var fillet = Api.Get("IFeatureManager.FeatureFillet3", () => fm.FeatureFillet3(195, .001, .001, 0, 0, 0, 0, null, null, null, null, null, null, null) as IFeature);
                Require(fillet != null, "Valid fillet fixture creation failed.");
                var data = Api.Get("IFeature.GetDefinition", () => fillet.GetDefinition() as ISimpleFilletFeatureData2);
                Api.Run("ISimpleFilletFeatureData2.DefaultRadius", () => data.DefaultRadius = .5);
                Api.Get("IFeature.ModifyDefinition", () => fillet.ModifyDefinition(data, doc, null));
                Api.Get("IModelDoc2.ForceRebuild3", () => doc.ForceRebuild3(false));
                Require(Has("feature"), "SOLIDWORKS rejected the large radius without retaining a failed feature.");
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
        }
        static void AddinCases()
        {
            string dll = @"C:\Program Files\IDEA\Unstuck\IdeaUnstuck.AddIn.dll";
            Case("installed add-in loads through SOLIDWORKS", () => {
                int result = Api.Get("ISldWorks.LoadAddIn", () => app.LoadAddIn(dll), -1);
                Console.WriteLine("LoadAddIn="+result); Require(result == 0 || result == 1, "LoadAddIn error " + result);
                Require(Api.Get("ISldWorks.GetAddInObject", () => app.GetAddInObject("IdeaUnstuck.AddIn")) != null, "Add-in object missing.");
            });
            Case("pane handles new part and document close", () => { NewPart(); Begin(); Rect(); Rescue.Run(app,"exit"); for(int i=0;i<25;i++){Application.DoEvents();System.Threading.Thread.Sleep(100);} Require(Read().BodyCount==0,"Unexpected state."); });
        }
    }
}
