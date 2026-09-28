using System;
using System.Collections.Generic;
using System.Linq;

namespace IdeaUnstuck.Core
{
    public sealed class StateSnapshot
    {
        public DateTime Utc { get; set; } = DateTime.UtcNow;
        public string DocumentKey { get; set; } = "";
        public int DocumentType { get; set; }
        public string FileName { get; set; } = "";
        public string Path { get; set; } = "";
        public string Version { get; set; } = "";
        public int UnitSystem { get; set; } = -1;
        public int LengthUnit { get; set; } = -1;
        public int DecimalPlaces { get; set; } = -1;
        public bool InSketch { get; set; }
        public SketchState Sketch { get; set; }
        public SketchState LastSketch { get; set; }
        public bool Rollback { get; set; }
        public int BodyCount { get; set; } = -1;
        public List<FeatureState> Features { get; set; } = new List<FeatureState>();
        public List<string> ReadIssues { get; set; } = new List<string>();
        public bool Saved => !string.IsNullOrEmpty(Path);
        public string BodySignature { get; set; } = "";
        public string StatusLine => DocumentType == 0 ? "Open a part with File > Open." :
            DocumentType != 1 ? "This helper works on parts. Open your .SLDPRT." :
            "Part: " + FileName + ", " + (InSketch ? "in a sketch on " + (Sketch?.Support ?? "an unknown surface") : "outside a sketch") + ", " + (LengthUnit == 3 ? "inches" : LengthUnit == 0 ? "millimeters" : "other units");
        public string PlainWords() => StatusLine + ". " + (Saved ? "Saved. " : "Never saved. ") +
            "Solid shapes: " + BodyCount + ". " + (Sketch ?? LastSketch)?.Describe() +
            " Rollback bar: " + (Rollback ? "above the bottom" : "at the bottom") + ". " +
            string.Join(" ", Features.Where(f => f.ErrorCode != 0).Select(f => f.Name + ": " + f.ErrorMeaning)) +
            (ReadIssues.Count > 0 ? " Some state could not be read: " + string.Join(", ", ReadIssues) : "");
    }
    public sealed class SketchState
    {
        public string Name { get; set; } = "";
        public string Support { get; set; } = "unknown surface";
        public string SupportKind { get; set; } = "unknown";
        public int EntityCount { get; set; }
        public int ContourCount { get; set; }
        public int ClosedContours { get; set; }
        public int OpenContours { get; set; }
        public int ConstraintCode { get; set; } = -1;
        public string ConstraintName { get; set; } = "Unknown";
        public string ConstraintMeaning { get; set; } = "Ask a TA.";
        public int ErrorCode { get; set; }
        public int UsageCode { get; set; }
        public string UsageMeaning { get; set; } = "";
        public int DimensionCount { get; set; }
        public string DimensionSignature { get; set; } = "";
        // Regions describe faces bounded by contours. A region with several loops can contain holes.
        public int RegionCount { get; set; }
        public string Describe() => "Sketch " + Name + " has " + EntityCount + " drawing items, " + ClosedContours + " closed outlines and " + OpenContours + " open outlines. " + ConstraintMeaning + (UsageCode != 0 ? " " + UsageMeaning : "");
    }
    public sealed class FeatureState
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public int ErrorCode { get; set; }
        public bool Warning { get; set; }
        public string ErrorMeaning { get; set; }
        public bool Suppressed { get; set; }
        public bool RolledBack { get; set; }
        public bool MidPlane { get; set; }
    }
    public sealed class Diagnosis
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Meaning { get; set; }
        public string NextClick { get; set; }
        public string Action { get; set; }
        public bool IsError { get; set; }
        public override string ToString() => Title + " " + Meaning + " " + NextClick;
    }
}
