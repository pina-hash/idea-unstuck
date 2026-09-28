using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using IdeaUnstuck.Core;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace IdeaUnstuck.SelfTest
{
    static partial class Program
    {
        static void StressCases(bool soak)
        {
            // Never measure an older installed DLL and attribute it to the new source.
            dynamic addin = Api.Get("ISldWorks.GetAddInObject", () => app.GetAddInObject("IdeaUnstuck.AddIn"));
            string expected = typeof(IdeaUnstuck.AddIn.Addin).Assembly.ManifestModule.ModuleVersionId.ToString();
            Dictionary<string, object> Metrics() => new JavaScriptSerializer().Deserialize<Dictionary<string, object>>((string)addin.GetMetrics());
            if (addin == null) { Skip("stress: install this build and load IDEA Unstuck first. Needs Mr. Pina."); return; }
            try { if ((string)Metrics()["build"] != expected) { Skip("stress: installed add-in differs from this build. Needs Mr. Pina."); return; } }
            catch (Exception) { Skip("stress: installed add-in has no current metrics endpoint. Needs Mr. Pina."); return; }
            if (app.GetDocumentCount() != 0) { Skip("stress: close existing documents first; no student document will be touched."); return; }
            Case(soak ? "45-minute modeled soak with in-host tick metrics" : "500 seeded random legal operations with installed add-in", () => {
                addin.ResetMetrics(); var random = new Random(209); var elapsed = Stopwatch.StartNew();
                var process = Process.GetProcessesByName("SLDWORKS").Single();
                long start = process.PrivateMemorySize64;
                var memory = new List<long> { start }; var history = new Dictionary<string,int>(); int operations = 0;
                while (soak ? elapsed.Elapsed < TimeSpan.FromMinutes(45) : operations < 500) {
                    string action = ModelOperation(random);
                    history[action] = history.TryGetValue(action, out var n) ? n+1 : 1; operations++;
                    var state = Read();
                    Require(!string.IsNullOrEmpty(state.Version), "Host stopped responding.");
                    if (operations % 10 == 0) { process.Refresh(); memory.Add(process.PrivateMemorySize64); Console.WriteLine("Stress operations="+operations+"; elapsed="+elapsed.Elapsed); }
                    for (int i=0;i<16;i++) { Application.DoEvents(); Thread.Sleep(100); }
                    var status = ((string)addin.GetStatus()).Split('|'); Require(status[0] == "connected" && status[3] == "0", "Host disconnected or logged an exception.");
                }
                var metrics = Metrics(); process.Refresh(); long end = process.PrivateMemorySize64;
                var poll = (Dictionary<string,object>)metrics["poll"];
                var report = new { mode = soak ? "soak" : "chaos", operations, minutes = elapsed.Elapsed.TotalMinutes, seed = 209, history, startPrivateBytes = start, endPrivateBytes = end, memorySamples = memory, metrics };
                File.WriteAllText(System.IO.Path.Combine(Output, soak ? "soak.json" : "chaos.json"), new JavaScriptSerializer().Serialize(report));
                Require(Convert.ToInt32(metrics["blankCards"]) == 0 && Convert.ToInt32(metrics["exceptions"]) == 0, "Blank card or caught exception.");
                Require(Convert.ToInt32(poll["MaxConcurrent"]) <= 1 && Convert.ToInt32(poll["Count"]) > 0, "No poll evidence or overlap.");
                if (soak) {
                    Require(Convert.ToDouble(poll["p95Ms"]) <= 50, "In-host poll p95 exceeded 50 ms.");
                    // A finite run cannot prove an asymptotic bound. Flag sustained growth
                    // beyond a generous 256 MiB from the warmed first quarter as a leak suspect.
                    long warm = memory[Math.Min(memory.Count-1,memory.Count/4)];
                    Require(end-warm <= 256L*1024*1024, "Memory growth exceeded the 256 MiB warmed-run limit.");
                }
                Console.WriteLine("Stress operations=" + operations + "; metrics=" + new JavaScriptSerializer().Serialize(metrics));
            });
        }
        static string ModelOperation(Random random)
        {
            var state = Reader.Read(app);
            if (doc == null || state.DocumentType != 1) { NewPart(); return "new part"; }
            var choices = new List<string> { "new part", "undo", "close", "assembly" };
            if (state.InSketch) { choices.Add("line"); choices.Add("circle"); choices.Add("exit"); if(state.Sketch.EntityCount==1 && state.Sketch.FullCircleCount==1 && state.Sketch.DimensionCount==0) choices.Add("dimension"); }
            else {
                if (state.LastSketch == null && state.BodyCount==0) choices.Add("sketch");
                if (state.LastSketch != null) choices.Add("edit sketch");
                if(state.BodyCount==0 && state.LastSketch?.ClosedContours==1 && state.LastSketch.OpenContours==0) choices.Add("extrude");
                if (state.BodyCount > 0) { choices.Add("rollback"); choices.Add("fillet"); choices.Add("cut"); }
            }
            string action = choices[random.Next(choices.Count)];
            switch(action) {
                case "new part": NewPart(); break;
                case "sketch": Begin(); break;
                case "line": Require(Manager().CreateLine(0,0,0,.01 + random.NextDouble()*.02,.02,0)!=null,"Line failed."); break;
                case "circle": Require(Manager().CreateCircleByRadius(random.NextDouble()*.02,0,0,.003)!=null,"Circle failed."); break;
                case "dimension": var segment=(ISketchSegment)((object[])Manager().ActiveSketch.GetSketchSegments())[0]; segment.Select4(false,null); Require(AddCircleDimension(true)!=null,"Dimension failed."); break;
                case "exit": Rescue.Run(app,"exit"); break;
                case "edit sketch": Rescue.Run(app,"edit"); break;
                case "extrude": Extrude(); break;
                case "undo": Rescue.Run(app,"undo"); break;
                case "rollback": var fm=doc.FeatureManager; Require(fm.EditRollback((int)swMoveRollbackBarTo_e.swMoveRollbackBarToBeforeFeature, Reader.Read(app).Features.Last(f=>f.Kind=="Extrusion" || f.Kind=="Boss").Name),"Rollback failed."); Rescue.Run(app,"roll"); break;
                case "fillet": NewPart(); Begin(); Rect(); Extrude(); AddSmallFillet(); break;
                case "cut": NewPart(); Begin(); Rect(); Extrude(); AddRoundCut(); break;
                case "close": var title=doc.GetTitle(); app.CloseDoc(title); owned.Remove(title); doc=null; break;
                case "assembly": var assembly=(IModelDoc2)app.NewDocument(assemblyTemplate,0,0,0); Require(assembly!=null,"Assembly failed."); var name=assembly.GetTitle(); owned.Add(name); Require(Has("wrong-document"),"Assembly message missing."); app.CloseDoc(name); owned.Remove(name); break;
            }
            return action;
        }
        static void AddSmallFillet()
        {
            var body=(IBody2)((object[])((IPartDoc)doc).GetBodies2(0,false))[0];
            var edges=(object[])body.GetEdges(); var fm=doc.FeatureManager;
            var data=(ISimpleFilletFeatureData2)fm.CreateDefinition((int)swFeatureNameID_e.swFmFillet);
            Require(data.Initialize((int)swSimpleFilletType_e.swConstRadiusFillet),"Fillet initialize failed.");
            data.ConicTypeForCrossSectionProfile=(int)swFeatureFilletProfileType_e.swFeatureFilletCircular; data.Edges=new object[]{edges[0]}; data.IsMultipleRadius=false; data.DefaultRadius=.0005;
            Require(fm.CreateFeature(data)!=null,"Fillet failed.");
        }
        static void AddRoundCut()
        {
            doc.ClearSelection2(true); Require(doc.Extension.SelectByID2("Front Plane","PLANE",0,0,0,false,0,null,0),"Plane not found.");
            Manager().InsertSketch(true); Manager().CreateCircleByRadius(.0127,.0127,0,.003); Manager().InsertSketch(true);
            var sketch=Reader.Read(app).LastSketch; ((IFeature)((IPartDoc)doc).FeatureByName(sketch.Name)).Select2(false,0);
            Require(doc.FeatureManager.FeatureCut4(true,false,false,1,1,.05,.05,false,false,false,false,0,0,false,false,false,false,false,true,true,true,true,false,0,0,false,false)!=null,"Cut failed.");
        }
    }
}
