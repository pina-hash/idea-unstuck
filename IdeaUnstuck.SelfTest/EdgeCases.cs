using System;
using System.IO;
using System.Linq;
using IdeaUnstuck.Core;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace IdeaUnstuck.SelfTest
{
    static partial class Program
    {
        static void EdgeCases()
        {
            Case("edge: no document has a useful message", () => {
                Require(app.GetDocumentCount()==0,"Close existing documents before the edge suite.");
                Require(new Rules().Evaluate(Read()).Single().Id=="no-document","No-document message missing.");
            });
            foreach (var count in new[]{20,300}) {
                Case("edge: " + count + " sketch features", () => {
                    NewPart();
                    for (int i=0;i<count;i++) {
                        doc.ClearSelection2(true); Require(doc.Extension.SelectByID2("Front Plane","PLANE",0,0,0,false,0,null,0),"Plane unavailable.");
                        Manager().InsertSketch(true); Manager().CreateLine(0,0,0,.01,.01,0); Manager().InsertSketch(true);
                        if ((i+1)%50==0) Console.WriteLine("Created edge fixture sketches="+(i+1));
                    }
                    var watch=System.Diagnostics.Stopwatch.StartNew(); var s=Read();
                    Require(s.Features.Count(f=>f.Kind=="ProfileFeature")>=count,"Feature count incorrect.");
                    Console.WriteLine("Edge sketches="+count+"; features="+s.Features.Count+"; external read ms="+watch.Elapsed.TotalMilliseconds);
                });
            }
            Case("edge: non-English sketch and plane names", () => {
                NewPart(); Begin(); Rect(); Rescue.Run(app,"exit");
                var sk=(IFeature)((IPartDoc)doc).FeatureByName(Read().LastSketch.Name); sk.Name="草図_Ñ_日本語";
                var plane=(IFeature)((IPartDoc)doc).FeatureByName("Front Plane"); plane.Name="前面_Frente";
                Rescue.Run(app,"edit"); var state=Read();
                Require(state.Sketch.Name=="草図_Ñ_日本語" && !new Rules().Evaluate(state).Any(d=>d.Id=="wrong-plane"),"Renamed front plane misdiagnosed.");
            });
            Case("edge: read-only saved part stays unchanged", () => {
                NewPart(); Begin(); Rect(); Extrude();
                string file=System.IO.Path.Combine(Output,"readonly-"+Guid.NewGuid().ToString("N")+".SLDPRT"); int errors=0,warnings=0;
                Require(doc.Extension.SaveAs(file,0,1,null,ref errors,ref warnings),"Save failed.");
                string title=doc.GetTitle(); owned.Add(title); app.CloseDoc(title); owned.Remove(title); doc=null;
                File.SetAttributes(file,File.GetAttributes(file)|FileAttributes.ReadOnly);
                try {
                    doc=(IModelDoc2)app.OpenDoc6(file,1,(int)swOpenDocOptions_e.swOpenDocOptions_Silent | (int)swOpenDocOptions_e.swOpenDocOptions_ReadOnly,"",ref errors,ref warnings);
                    Require(doc!=null && errors==0,"Read-only open failed."); owned.Add(doc.GetTitle());
                    var before=Read(); var after=Read(); Require(before.BodySignature==after.BodySignature && before.Saved,"Read-only part changed.");
                    string openedTitle=doc.GetTitle(); app.CloseDoc(openedTitle); owned.Remove(openedTitle); doc=null;
                } finally { File.SetAttributes(file,FileAttributes.Normal); }
            });
            Case("edge: drawing is declined safely", () => {
                string template=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(partTemplate),"Drawing.DRWDOT");
                Require(File.Exists(template),"Drawing template missing.");
                var drawing=(IModelDoc2)app.NewDocument(template,0,0,0); Require(drawing!=null,"Drawing failed.");
                string title=drawing.GetTitle(); owned.Add(title);
                Require(Has("wrong-document"),"Drawing message missing."); app.CloseDoc(title); owned.Remove(title);
            });
            Case("edge: stress cut and fillet fixtures", () => {
                NewPart(); Begin(); Rect(); Extrude(); AddRoundCut(); Require(Read().BodyCount==1,"Cut did not leave one body.");
                NewPart(); Begin(); Rect(); Extrude(); AddSmallFillet(); Require(Read().Features.Any(f=>f.Kind=="Fillet" && f.ErrorCode==0),"Fillet fixture failed.");
            });
            Skip("edge: network path requires a supplied reachable school share; no credentials or network drive were invented.");
        }
    }
}
