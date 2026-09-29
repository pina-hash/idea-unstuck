using System;
using System.Collections.Generic;
using System.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace IdeaUnstuck.Core
{
    public sealed class RescueActions
    {
        readonly ApiGuard api;
        readonly Telemetry log;
        readonly StateReader reader;
        public RescueActions(Telemetry log) { this.log = log; api = new ApiGuard(log); reader = new StateReader(log); }
        public string Run(ISldWorks app, string action)
        {
            log.Write("button_pressed", new { action });
            return api.Get("RescueActions." + action, () => Execute(app, action), "That click did not finish. Press Copy for AI or ask a TA.");
        }
        string Execute(ISldWorks app, string action)
        {
            if (action == "newpart")
            {
                var template = api.Get("ISldWorks.GetUserPreferenceStringValue", () => app.GetUserPreferenceStringValue((int)swUserPreferenceStringValue_e.swDefaultTemplatePart), "");
                if (string.IsNullOrEmpty(template) || !System.IO.File.Exists(template)) return "Click File > New > Part. The helper could not find the part template.";
                var made = api.Get("ISldWorks.NewDocument", () => app.NewDocument(template, 0, 0, 0) as IModelDoc2);
                log.Write("rescue_result", new { action, success = made != null });
                return made != null ? "A new part is open. Next: open the Practice tab, or start drawing." : "Click File > New > Part. The helper could not start a new part.";
            }
            var doc = api.Get("ISldWorks.ActiveDoc", () => app.ActiveDoc as IModelDoc2);
            if (doc == null || api.Get("IModelDoc2.GetType", () => doc.GetType()) != 1) return "Open a part with File > Open first.";
            var manager = api.Get("IModelDoc2.SketchManager", () => doc.SketchManager);
            if (manager == null) return "The sketch tools are busy. Try this button again.";
            bool editing = api.Get("ISketchManager.ActiveSketch", () => manager.ActiveSketch != null);
            if (action == "undo") return api.Run("IModelDoc2.EditUndo2", () => doc.EditUndo2(1)) ? "Undo was sent. Check your part." : "Undo did not finish. Ask a TA.";
            if ((action == "front" || action == "edit") && editing) return "You are already in a sketch.";
            if ((action == "roll" || action == "find") && editing) return "Press Exit sketch first.";
            if (action == "exit" && !editing) return "You are already outside your sketch.";
            var extension = api.Get("IModelDoc2.Extension", () => doc.Extension);
            // View, selection, sketch-mode, and rollback changes are not reliably undoable in SOLIDWORKS.
            bool record = action == "units" && extension != null && api.Run("IModelDocExtension.StartRecordingUndoObject", () => extension.StartRecordingUndoObject());
            bool ok = true;
            try
            {
                switch (action)
                {
                    case "find":
                        ok = api.Run("IModelDoc2.ShowNamedView2", () => doc.ShowNamedView2("*Isometric", 7));
                        ok &= api.Run("IModelDoc2.ViewZoomtofit2", () => doc.ViewZoomtofit2());
                        break;
                    case "units":
                        ok = api.Get("IModelDoc2.SetUserPreferenceIntegerValue.swUnitSystem", () => doc.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swUnitSystem, (int)swUnitSystem_e.swUnitSystem_IPS));
                        ok &= api.Get("IModelDoc2.SetUserPreferenceIntegerValue.swUnitsLinearDecimalPlaces", () => doc.SetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalPlaces, 3));
                        break;
                    case "exit": ok = api.Run("ISketchManager.InsertSketch", () => manager.InsertSketch(true)); break;
                    case "roll":
                        var fm = api.Get("IModelDoc2.FeatureManager", () => doc.FeatureManager);
                        ok = fm != null && api.Get("IFeatureManager.EditRollback", () => fm.EditRollback((int)swMoveRollbackBarTo_e.swMoveRollbackBarToEnd, ""));
                        break;
                    case "front":
                        var initial = reader.Read(app);
                        if (initial.BodyCount != 0 || initial.LastSketch != null) return "This part already has a sketch or shape. Press Get back into my sketch.";
                        var plane = api.Get("IPartDoc.FeatureByName", () => ((IPartDoc)doc).FeatureByName("Front Plane") as IFeature);
                        if (plane == null) return "Front Plane was not found. Ask a TA to check the part template.";
                        ok = api.Run("IModelDoc2.ClearSelection2", () => doc.ClearSelection2(true));
                        ok &= api.Get("IFeature.Select2", () => plane.Select2(false, 0));
                        if (ok) ok = api.Run("ISketchManager.InsertSketch", () => manager.InsertSketch(true));
                        break;
                    case "edit":
                        var selection = api.Get("IModelDoc2.SelectionManager", () => doc.SelectionManager as ISelectionMgr);
                        IFeature chosen = null;
                        if (selection != null && api.Get("ISelectionMgr.GetSelectedObjectCount2", () => selection.GetSelectedObjectCount2(-1)) > 0)
                            chosen = api.Get("ISelectionMgr.GetSelectedObject6", () => selection.GetSelectedObject6(1, -1) as IFeature);
                        string kind = chosen == null ? "" : api.Get("IFeature.GetTypeName2", () => chosen.GetTypeName2(), "");
                        if (kind != "ProfileFeature" && kind != "3DProfileFeature")
                        {
                            string name = reader.Read(app).LastSketch?.Name;
                            chosen = name == null ? null : api.Get("IPartDoc.FeatureByName", () => ((IPartDoc)doc).FeatureByName(name) as IFeature);
                        }
                        if (chosen == null) return "No sketch was found. Press Start a sketch on the Front Plane.";
                        ok = api.Run("IModelDoc2.ClearSelection2", () => doc.ClearSelection2(true));
                        ok &= api.Get("IFeature.Select2", () => chosen.Select2(false, 0));
                        if (ok) ok = api.Run("IModelDoc2.EditSketch", () => doc.EditSketch());
                        break;
                    default: return "This button is not available.";
                }
            }
            finally { if (record) api.Get("IModelDocExtension.FinishRecordingUndoObject", () => extension.FinishRecordingUndoObject("IDEA Unstuck: inches")); }
            log.Write("rescue_result", new { action, success = ok });
            return ok ? action == "units" ? "Units are inches. Existing shapes keep their size." : "Done. Check your part." : "That click did not finish. Press Copy for AI or ask a TA.";
        }
        public static string AiPrompt(StateSnapshot snapshot, IEnumerable<Diagnosis> diagnoses)
        {
            int revision;
            string version = int.TryParse(snapshot.Version.Split('.')[0], out revision) ? (1992 + revision).ToString() : snapshot.Version;
            return "I am a beginner in SOLIDWORKS " + version + ". Here is my exact situation: " + snapshot.PlainWords() + "\r\n" +
                string.Join("\r\n", diagnoses.Select(d => d.ToString())) +
                "\r\nTell me what is CAUSING this and the next click, one step at a time. I will paste a screenshot of my whole window next.";
        }
    }
}
