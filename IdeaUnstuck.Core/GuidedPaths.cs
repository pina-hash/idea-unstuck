using System;
using System.Collections.Generic;
using System.Linq;

namespace IdeaUnstuck.Core
{
    public sealed class PathStep
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Action { get; set; }
        public bool Done { get; set; }
        public string Hint { get; set; }
    }
    public sealed class GuidedPath
    {
        public string Title { get; set; }
        public List<PathStep> Steps { get; set; }
    }
    public sealed class GuidedPaths
    {
        readonly Telemetry log;
        string document;
        string beforeDimensions, beforeBody;
        bool changedSize, sawFaceSketch, sawRound;
        StateSnapshot lastSolidState;
        HashSet<string> initialCuts = new HashSet<string>();
        public List<GuidedPath> Paths { get; private set; }
        public GuidedPaths(Telemetry log) { this.log = log; Reset(); }
        public void Reset()
        {
            document = null; beforeDimensions = null; beforeBody = null; changedSize = false; sawFaceSketch = false; sawRound = false; lastSolidState = null; initialCuts.Clear();
            PathStep S(string id, string text, string action = null) => new PathStep { Id = id, Text = text, Action = action };
            Paths = new List<GuidedPath> {
                new GuidedPath { Title = "My first 3D part", Steps = new List<PathStep> {
                    S("a1", "Click File > New > Part."), S("a2", "Press Set units to inches.", "units"),
                    S("a3", "Click Front Plane. Click Sketch.", "front"), S("a4", "Click Rectangle. Draw one closed outline."),
                    S("a5", "Click Smart Dimension. Click a line. Place the size label. Type a value."), S("a6", "Click Exit Sketch.", "exit"),
                    S("a7", "Click Features > Extruded Boss/Base. Choose Mid Plane. Click the green check."),
                    S("a8", "Check that Solid Bodies in the left tree shows 1."), S("a9", "Press Ctrl+S. Include your last name in the file name.") } },
                new GuidedPath { Title = "Change a size", Steps = new List<PathStep> {
                    S("b1", "Press Get back into my sketch."), S("b2", "Double-click a size. Type a new value. Press Enter."),
                    S("b3", "Click Exit Sketch.", "exit"), S("b4", "Check that the 3D shape changed.") } },
                new GuidedPath { Title = "Cut into a face", Steps = new List<PathStep> {
                    S("c1", "Click a flat face. Click Sketch."), S("c2", "Click Circle or Rectangle. Draw a closed outline."),
                    S("c3", "Click Exit Sketch. Click Features > Extruded Cut. Click the green check."), S("c4", "Check that Solid Bodies still shows 1.") } },
                new GuidedPath { Title = "Round an edge", Steps = new List<PathStep> {
                    S("d1", "Click Features > Fillet. Pick an edge. Type a radius: the size of the round. Click the green check."),
                    S("d2", "Check that Fillet has no red X in the left tree.") } }
            };
            Paths.Add(new GuidedPath { Title = "First 10 minutes", Steps = new List<PathStep> {
                new PathStep { Id="e1", Text="Click File > New > Part.", Hint="Top-left menu: File > New. Choose Part." },
                new PathStep { Id="e2", Text="Check that the units are inches.", Action="units", Hint="Bottom-right status bar: look for IPS. In this pane, Set units to inches changes it." },
                new PathStep { Id="e3", Text="Click Front Plane. Click Sketch.", Action="front", Hint="Left tree: Front Plane. Top ribbon: Sketch tab, then Sketch." },
                new PathStep { Id="e4", Text="Click Circle. Start at the origin.", Hint="Top Sketch tab: Circle. In the drawing area, click the red origin point. It is the starting point." },
                new PathStep { Id="e5", Text="Click Smart Dimension. Set the circle's width to 1.000.", Hint="Top Sketch tab: Smart Dimension. Click the circle edge. Place the label. Type 1.000. Press Enter. The diameter symbol means full width." },
                new PathStep { Id="e6", Text="Click Exit Sketch.", Action="exit", Hint="Top-left Sketch tab: Exit Sketch. This pane also has Exit sketch." },
                new PathStep { Id="e7", Text="Click Extruded Boss/Base. Choose Mid Plane. Set 0.500. Click the green check.", Hint="Top Features tab: Extruded Boss/Base. Left settings panel: Direction 1, Mid Plane, depth 0.500 inches." },
                new PathStep { Id="e8", Text="Press Ctrl+S. Put your last name in the file name.", Hint="Keyboard: hold Ctrl and press S. The save window lets you choose your class folder and name." }
            }});
        }
        public void Update(StateSnapshot s)
        {
            if (document != s.DocumentKey) { Reset(); document = s.DocumentKey; }
            if (s.DocumentType != 1 || s.ReadIssues.Count > 0) return;
            void Mark(string id, bool complete)
            {
                var step = Paths.SelectMany(p => p.Steps).First(x => x.Id == id);
                if (complete && !step.Done) { step.Done = true; log.Write("guided_step_completed", new { id, document = s.FileName }); }
            }
            bool Done(string id) => Paths.SelectMany(p => p.Steps).First(x => x.Id == id).Done;
            var sketch = s.Sketch ?? s.LastSketch;
            Mark("e1", true); Mark("e2", s.UnitSystem==3 && s.LengthUnit==3);
            Mark("e3", s.InSketch && sketch?.SupportKind=="plane" && sketch.Support==(s.FrontPlaneName.Length>0?s.FrontPlaneName:"Front Plane"));
            Mark("e4", Done("e3") && sketch?.EntityCount==1 && sketch.FullCircleCount==1 && sketch.CircleAtOrigin);
            Mark("e5", Done("e4") && sketch?.InchCircleAtOrigin == true && sketch.DiameterDimensionCount>0);
            Mark("e6", Done("e5") && !s.InSketch);
            Mark("e7", Done("e6") && s.BodyCount==1 && s.Features.Any(f=>f.MidPlane && Math.Abs(f.Depth-.0127)<1e-7 && !f.Suppressed && !f.RolledBack && f.ErrorCode==0));
            Mark("e8", Done("e7") && s.Saved);
            bool outline = sketch != null && sketch.ClosedContours == 1 && sketch.OpenContours == 0;
            Mark("a1", true); Mark("a2", s.UnitSystem == 3 && s.LengthUnit == 3);
            Mark("a3", s.InSketch && sketch?.Support == "Front Plane");
            Mark("a4", Done("a3") && outline); Mark("a5", Done("a4") && sketch?.DimensionCount > 0);
            Mark("a6", Done("a5") && !s.InSketch);
            Mark("a7", Done("a6") && s.Features.Any(f => f.MidPlane && !f.Suppressed && !f.RolledBack && f.ErrorCode == 0));
            Mark("a8", Done("a7") && s.BodyCount == 1); Mark("a9", Done("a8") && s.Saved);
            if (s.InSketch && lastSolidState != null && s.Sketch != null && s.Sketch.DimensionCount > 0)
            {
                Mark("b1", true);
                if (beforeDimensions == null) { beforeDimensions = s.Sketch.DimensionSignature; beforeBody = lastSolidState.BodySignature; }
                changedSize |= beforeDimensions != s.Sketch.DimensionSignature;
                Mark("b2", changedSize);
            }
            Mark("b3", changedSize && !s.InSketch);
            Mark("b4", Done("b3") && s.BodyCount > 0 && s.BodySignature != beforeBody && s.Features.All(f => f.ErrorCode == 0));
            if (s.InSketch && s.Sketch?.SupportKind == "face" && !sawFaceSketch)
            { sawFaceSketch = true; initialCuts = new HashSet<string>(s.Features.Where(f => f.Kind == "Cut").Select(f => f.Name)); }
            Mark("c1", sawFaceSketch); Mark("c2", sawFaceSketch && s.InSketch && s.Sketch?.SupportKind == "face" && outline);
            Mark("c3", Done("c2") && !s.InSketch && s.Features.Any(f => f.Kind == "Cut" && !initialCuts.Contains(f.Name) && f.ErrorCode == 0 && !f.Suppressed));
            Mark("c4", Done("c3") && s.BodyCount == 1);
            var fillets = s.Features.Where(f => f.Kind == "Fillet" && !f.Suppressed && !f.RolledBack).ToArray();
            sawRound |= fillets.Length > 0;
            Mark("d1", sawRound); Mark("d2", fillets.Length > 0 && fillets.All(f => f.ErrorCode == 0));
            if (!s.InSketch && s.BodyCount > 0) lastSolidState = s;
        }
    }
}
