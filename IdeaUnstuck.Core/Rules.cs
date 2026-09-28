using System;
using System.Collections.Generic;
using System.Linq;

namespace IdeaUnstuck.Core
{
    public sealed class Rules
    {
        public List<Diagnosis> Evaluate(StateSnapshot s)
        {
            var result = new List<Diagnosis>();
            void Add(string id, string title, string meaning, string next, string action = null, bool error = false) => result.Add(new Diagnosis { Id = id, Title = title, Meaning = meaning, NextClick = next, Action = action, IsError = error });
            if (s.DocumentType == 0) { Add("no-document", "Open a part.", "Start with a part file.", "Click File > New > Part."); return result; }
            if (s.DocumentType != 1) { Add("wrong-document", "You opened " + (s.DocumentType == 2 ? "an assembly." : "a drawing."), "This helper works on parts.", "Click File > Open and choose your .SLDPRT."); return result; }
            if (s.ReadIssues.Count > 0) Add("read-incomplete", "Some checks are unavailable.", "The helper could not read all of this part.", "Press Copy for AI, or ask a TA.", "copy");
            if (s.Rollback && !s.InSketch) Add("rollback", "Your rollback bar is not at the bottom.", "Features below it are switched off.", "Press Roll to end.", "roll", true);
            var sk = s.InSketch ? s.Sketch : s.BodyCount == 0 ? s.LastSketch : null;
            if (sk != null && sk.ConstraintCode >= 4 && sk.ConstraintCode <= 6) Add("over-defined", "Red means over-defined.", ErrorWords.Constraint(sk.ConstraintCode), "Press Undo.", "undo", true);
            foreach (var f in s.Features.Where(f => f.ErrorCode != 0 && !f.Suppressed && !f.RolledBack))
                Add("feature:" + f.Name + ":" + f.ErrorCode, (f.Warning ? "A feature has a warning: " : "A feature failed: ") + f.Name + ".", f.ErrorMeaning, "Right-click " + f.Name + " in the left tree and choose Edit Feature.", null, !f.Warning);
            if (s.LengthUnit == 0) Add("millimeters", "You are in millimeters.", "One inch is 25.4 millimeters. Check sizes you already drew.", "Press Set units to inches.", "units", true);
            else if (s.LengthUnit >= 0 && s.LengthUnit != 3) Add("other-units", "Your units are not inches.", "This class uses inches.", "Press Set units to inches.", "units", true);
            if (sk != null && sk.OpenContours > 0) Add("open-contour", "Your shape is not closed.", "A solid extrusion needs a closed outline.", "Drag one open end onto the other until they snap.", null, true);
            if (sk != null && sk.ClosedContours > 1) Add("multiple-outlines", "Your sketch has " + sk.ClosedContours + " closed outlines.", "An outline inside another can make a hole. Side-by-side outlines can make separate shapes.", "Delete any extra outline you did not intend.");
            if (!s.InSketch && s.LastSketch != null && s.BodyCount == 0 && !s.Rollback) Add("left-sketch", "You left your sketch.", "Open it to keep drawing or change a size.", "Press Get back into my sketch.", "edit");
            if (s.BodyCount > 1) Add("multiple-bodies", "This part has " + s.BodyCount + " separate 3D shapes.", "They are called solid bodies. This lesson needs one.", "Right-click the last Extrude in the left tree and choose Edit Feature.", null, true);
            if (s.BodyCount == 0 && s.LastSketch != null && !s.InSketch && !s.Rollback) Add("no-body", "No 3D shape yet.", "You have a sketch but no solid shape.", "Click Features > Extruded Boss/Base.");
            if (!s.Saved) Add("unsaved", "This part has never been saved.", "Give your work a name so you can find it.", "Press Ctrl+S and name it with your last name.");
            return result;
        }
    }
    public sealed class DiagnosisTracker
    {
        readonly Telemetry log;
        readonly Dictionary<string, DateTime> live = new Dictionary<string, DateTime>();
        readonly Queue<DateTime> undos = new Queue<DateTime>();
        string signature = "", document = "";
        DateTime unchangedSince;
        bool triggered;
        public DiagnosisTracker(Telemetry log) { this.log = log; }
        public void Undo(DateTime now) { undos.Enqueue(now); }
        public void Reset(DateTime now)
        {
            foreach (var pair in live) log.Write("diagnosis_cleared", new { id = pair.Key, seconds = (now - pair.Value).TotalSeconds });
            live.Clear(); undos.Clear(); signature = ""; document = ""; triggered = false;
        }
        public void Update(StateSnapshot s, List<Diagnosis> diagnoses, DateTime now)
        {
            if (document != s.DocumentKey) { Reset(now); document = s.DocumentKey; }
            foreach (var id in live.Keys.Except(diagnoses.Select(d => d.Id)).ToArray()) { log.Write("diagnosis_cleared", new { id, seconds = (now - live[id]).TotalSeconds }); live.Remove(id); }
            foreach (var d in diagnoses.Where(d => !live.ContainsKey(d.Id))) { live[d.Id] = now; log.Write("diagnosis_appeared", new { d.Id, d.Title }); }
            var errors = string.Join("|", diagnoses.Where(d => d.IsError).Select(d => d.Id));
            var next = errors + ":" + s.Sketch?.DimensionSignature + ":" + s.Sketch?.EntityCount + ":" + s.Sketch?.ContourCount;
            if (next != signature) { signature = next; unchangedSince = now; triggered = false; }
            while (undos.Count > 0 && (now - undos.Peek()).TotalSeconds > 60) undos.Dequeue();
            bool stuck = (errors.Length > 0 && (now - unchangedSince).TotalMinutes >= 3) || undos.Count >= 5;
            if (stuck)
            {
                diagnoses.Insert(0, new Diagnosis { Id = "stuck", Title = "Looks like you are stuck.", Meaning = "You can ask for help with your exact situation.", NextClick = "Press Copy for AI and paste it into your AI chat with a screenshot, or raise your hand for a TA.", Action = "copy" });
                if (!triggered) log.Write("stuck_triggered", new { reason = undos.Count >= 5 ? "five_undos" : "three_minutes", errors });
                triggered = true;
            }
            else triggered = false;
        }
    }
}
