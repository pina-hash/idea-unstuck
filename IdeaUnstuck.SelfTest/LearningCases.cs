using System;
using System.IO;
using System.Linq;
using IdeaUnstuck.Core;
using SolidWorks.Interop.sldworks;
using Environment = System.Environment;

namespace IdeaUnstuck.SelfTest
{
    static partial class Program
    {
        static void LearningCases()
        {
            Case("lesson: first ten minutes from real circle to saved Mid Plane body", () => {
                NewPart(); var paths=new GuidedPaths(Log); var progress=new ProgressTelemetry(Log);
                Log.Write("session_start",new {user=Environment.UserName,solidworks=app.RevisionNumber(),addin="SelfTest learning"});
                void Observe(){var s=Read();paths.Update(s);progress.Observe(s);}
                Observe(); Rescue.Run(app,"units");Observe();Begin();Observe();
                var circle=(ISketchSegment)Manager().CreateCircleByRadius(0,0,0,.0127);Require(circle!=null,"Circle missing.");Observe();
                circle.Select4(false,null);Require(AddCircleDimension(true)!=null,"Diameter dimension missing.");Observe();
                Extrude();Observe();
                string file=System.IO.Path.Combine(Output,"SELFTEST_LASTNAME_"+Guid.NewGuid().ToString("N")+".SLDPRT");int errors=0,warnings=0;
                Require(doc.Extension.SaveAs(file,0,1,null,ref errors,ref warnings),"Save failed.");owned.Add(doc.GetTitle());Observe();
                var lesson=paths.Paths.Single(p=>p.Title=="First 10 minutes");
                Require(lesson.Steps.All(s=>s.Done),"Missing steps: "+string.Join(",",lesson.Steps.Where(s=>!s.Done).Select(s=>s.Id)));
                Log.Write("session_end",new {user=Environment.UserName,solidworks=app.RevisionNumber(),addin="SelfTest learning"});
                // A student can Undo the whole sketch after the path has observed it.
                NewPart(); paths=new GuidedPaths(Log); Begin();
                Manager().CreateCircleByRadius(0,0,0,.0127); paths.Update(Read());
                Require(paths.Paths.Single(p=>p.Title=="First 10 minutes").Steps[3].Done,"Undo fixture never reached the circle step.");
                for(int i=0;i<10 && (Read().LastSketch!=null || Read().InSketch);i++) { Rescue.Run(app,"undo"); paths.Update(Read()); }
                Require(Read().LastSketch==null && !Read().InSketch,"Undo did not remove the fixture sketch.");
            });
            Case("stuck v2: native repeated Line starts and cancels", () => {
                NewPart();Begin();var tracker=new DiagnosisTracker(Log);tracker.Update(Read(),new System.Collections.Generic.List<Diagnosis>(),DateTime.UtcNow);
                var events=(DSldWorksEvents_Event)app;int starts=0,closes=0;
                DSldWorksEvents_CommandOpenPreNotifyEventHandler opened=(command,userCommand)=>{tracker.CommandOpened(command);if(command==43)starts++;return 0;};
                DSldWorksEvents_CommandCloseNotifyEventHandler closed=(command,reason)=>{tracker.CommandClosed(command,reason,DateTime.UtcNow);if(command==43 && reason==-1)closes++;return 0;};
                events.CommandOpenPreNotify+=opened;events.CommandCloseNotify+=closed;
                try {
                    for(int i=0;i<3;i++){Require(app.RunCommand(43,""),"Line did not start.");Require(app.RunCommand(-1,""),"Line did not cancel.");System.Windows.Forms.Application.DoEvents();}
                    var list=new System.Collections.Generic.List<Diagnosis>();tracker.Update(Read(),list,DateTime.UtcNow);
                    Console.WriteLine("Native Line starts="+starts+"; cancels="+closes);
                    Require(starts>=3 && closes>=3 && list.Any(d=>d.Id=="stuck"),"Native cancel trigger not observed.");
                } finally {events.CommandOpenPreNotify-=opened;events.CommandCloseNotify-=closed;app.RunCommand(-1,"");}
            });
        }
    }
}
