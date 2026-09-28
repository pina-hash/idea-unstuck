using System;
using System.Linq;
using IdeaUnstuck.Core;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace IdeaUnstuck.SelfTest
{
    static partial class Program
    {
        static void TrapCases()
        {
            Case("trap: hidden FeatureManager tree", () => {
                NewPart(); var ext = Api.Get("IModelDoc2.Extension", () => doc.Extension); int width = doc.GetFeatureManagerWidth();
                try {
                    Api.Get("IModelDocExtension.HideFeatureManager", () => ext.HideFeatureManager(true));
                    var state = Read(); Console.WriteLine("Tree width hidden=" + state.FeatureTreeWidth);
                    Require(new Rules().Evaluate(state).Any(d => d.Id == "tree-gone"), "Hidden tree cannot be inferred from this API.");
                } finally { Api.Get("IModelDocExtension.HideFeatureManager", () => ext.HideFeatureManager(width == 0)); }
            });
            Case("trap: selection filters restrict clicks", () => {
                NewPart(); bool edge = Api.Get("ISldWorks.GetSelectionFilter", () => app.GetSelectionFilter((int)swSelectType_e.swSelEDGES)); bool apply = Api.Get("ISldWorks.GetApplySelectionFilter", () => app.GetApplySelectionFilter());
                try {
                    Api.Run("ISldWorks.SetSelectionFilter", () => app.SetSelectionFilter((int)swSelectType_e.swSelEDGES, true));
                    Api.Run("ISldWorks.SetApplySelectionFilter", () => app.SetApplySelectionFilter(true));
                    Require(Has("wrong-select"), "Selection filter guidance missing.");
                } finally {
                    Api.Run("ISldWorks.SetSelectionFilter.restore", () => app.SetSelectionFilter((int)swSelectType_e.swSelEDGES,edge));
                    Api.Run("ISldWorks.SetApplySelectionFilter.restore", () => app.SetApplySelectionFilter(apply));
                }
            });
            Case("trap: first sketch on Top Plane", () => {
                NewPart();
                var ext = Api.Get("IModelDoc2.Extension", () => doc.Extension);
                Require(Api.Get("IModelDocExtension.SelectByID2", () => ext.SelectByID2("Top Plane", "PLANE", 0,0,0,false,0,null,0)), "Top Plane not found.");
                Api.Run("ISketchManager.InsertSketch", () => Manager().InsertSketch(true)); Rect();
                Require(Has("wrong-plane"), "Wrong-plane guidance missing.");
            });
            Case("trap: one-sided extrusion and zero sketch dimensions", () => {
                NewPart(); Begin(); Rect(); Extrude(true, false);
                Require(Has("extrude-direction") && Has("no-dimensions"), "Missing direction or size guidance.");
            });
            Case("trap: full circle radius dimension", () => {
                NewPart(); Begin();
                var circle = Api.Get("ISketchManager.CreateCircleByRadius", () => Manager().CreateCircleByRadius(0,0,0,.0127) as ISketchSegment);
                Require(circle != null && Api.Get("ISketchSegment.Select4", () => circle.Select4(false,null)), "Circle fixture failed.");
                var dimension = AddCircleDimension(false);
                Require(dimension != null, "Radius dimension failed.");
                Require(Has("radius-not-dia"), "Radius guidance missing.");
            });
            Case("trap: five minutes without a sketch (real part, injected clock)", () => {
                NewPart(); var now = DateTime.UtcNow;
                var reader = new StateReader(Log, () => now);
                Require(!new Rules().Evaluate(reader.Read(app)).Any(d => d.Id == "no-sketch"), "Triggered before five minutes.");
                now = now.AddMinutes(5);
                Require(new Rules().Evaluate(reader.Read(app)).Any(d => d.Id == "no-sketch"), "Missing five-minute guidance.");
                Begin(); Rect();
                Require(!new Rules().Evaluate(reader.Read(app)).Any(d => d.Id == "no-sketch"), "Guidance did not clear.");
            });
            Case("trap: rebuild needed after dimension change", () => {
                NewPart(); Begin(); Rect(); var boss = Extrude();
                var display = Api.Get("IFeature.GetFirstDisplayDimension", () => boss.GetFirstDisplayDimension() as IDisplayDimension);
                var dimension = Api.Get("IDisplayDimension.GetDimension2", () => display.GetDimension2(0) as IDimension);
                Api.Run("IDimension.SystemValue", () => dimension.SystemValue = .02);
                Require(Has("rebuild"), "Missing rebuild guidance.");
                Api.Get("IModelDoc2.EditRebuild3", () => doc.EditRebuild3());
                Require(!Has("rebuild"), "Rebuild guidance did not clear.");
            });
            Case("trap: active Line tool", () => {
                NewPart(); Begin();
                try {
                    Require(Api.Get("ISldWorks.RunCommand", () => app.RunCommand(43,"")), "Line command did not start.");
                    var s = Read(); Console.WriteLine("Command=" + s.ActiveCommand + "; active=" + s.CommandActive);
                    Require(new Rules().Evaluate(s).Any(d => d.Id == "still-drawing"), "Active tool guidance missing.");
                } finally { Api.Get("ISldWorks.RunCommand", () => app.RunCommand(767,"")); }
            });
            Case("trap: SketchXpert availability in over-defined sketch", () => {
                NewPart(); Begin();
                var line = Api.Get("ISketchManager.CreateLine", () => Manager().CreateLine(0,0,0,.0254,0,0) as ISketchSegment);
                Api.Get("ISketchSegment.Select4", () => line.Select4(false,null)); Api.Run("IModelDoc2.SketchAddConstraints", () => doc.SketchAddConstraints("sgFIXED"));
                Api.Get("ISketchSegment.Select4", () => line.Select4(false,null)); Api.Run("IModelDoc2.SketchAddConstraints", () => doc.SketchAddConstraints("sgVERTICAL"));
                var s = Read(); Require(s.SketchXpertAvailable, "SketchXpert was not enabled.");
                Require(new Rules().Evaluate(s).Single(d => d.Id == "over-defined").NextClick.Contains("SketchXpert"), "Missing SketchXpert help.");
            });
        }
        static IDisplayDimension AddCircleDimension(bool diameter)
        {
            bool input = Api.Get("ISldWorks.GetUserPreferenceToggle", () => app.GetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate));
            try {
                Api.Run("ISldWorks.SetUserPreferenceToggle", () => app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, false));
                return diameter ? Api.Get("IModelDoc2.AddDiameterDimension2", () => doc.AddDiameterDimension2(.02,.02,0) as IDisplayDimension)
                    : Api.Get("IModelDoc2.AddRadialDimension2", () => doc.AddRadialDimension2(.02,.02,0) as IDisplayDimension);
            } finally { Api.Run("ISldWorks.SetUserPreferenceToggle", () => app.SetUserPreferenceToggle((int)swUserPreferenceToggle_e.swInputDimValOnCreate, input)); }
        }
    }
}
