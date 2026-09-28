using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace IdeaUnstuck.Core
{
    public sealed class StateReader
    {
        readonly ApiGuard api;
        public StateReader(Telemetry log) { api = new ApiGuard(log); }
        public StateSnapshot Read(ISldWorks app)
        {
            var s = new StateSnapshot();
            return api.Get("StateReader.Read", () => ReadInternal(app, s), s, s.ReadIssues);
        }
        StateSnapshot ReadInternal(ISldWorks app, StateSnapshot s)
        {
            T Read<T>(string name, Func<T> action, T fallback = default(T)) => api.Get(name, action, fallback, s.ReadIssues);
            var doc = Read("ISldWorks.ActiveDoc", () => app.ActiveDoc as IModelDoc2);
            s.Version = Read("ISldWorks.RevisionNumber", () => app.RevisionNumber(), "unknown");
            if (doc == null) return s;
            s.DocumentType = Read("IModelDoc2.GetType", () => doc.GetType());
            s.FileName = Read("IModelDoc2.GetTitle", () => doc.GetTitle(), "Unnamed part");
            s.Path = Read("IModelDoc2.GetPathName", () => doc.GetPathName(), "");
            s.DocumentKey = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(doc).ToString(CultureInfo.InvariantCulture);
            if (s.DocumentType != 1) return s;
            s.UnitSystem = Read("IModelDoc2.GetUserPreferenceIntegerValue.swUnitSystem", () => doc.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swUnitSystem), -1);
            s.LengthUnit = Read("IModelDoc2.GetUserPreferenceIntegerValue.swUnitsLinear", () => doc.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swUnitsLinear), -1);
            s.DecimalPlaces = Read("IModelDoc2.GetUserPreferenceIntegerValue.swUnitsLinearDecimalPlaces", () => doc.GetUserPreferenceIntegerValue((int)swUserPreferenceIntegerValue_e.swUnitsLinearDecimalPlaces), -1);
            var manager = Read("IModelDoc2.SketchManager", () => doc.SketchManager);
            var sketch = manager == null ? null : Read("ISketchManager.ActiveSketch", () => manager.ActiveSketch as ISketch);
            s.InSketch = sketch != null;
            if (sketch != null) s.Sketch = ReadSketch(sketch, s);
            var seen = new HashSet<string>();
            int count = 0;
            void Walk(IFeature feature, bool sub, int depth)
            {
                while (feature != null && count++ < 1500 && depth < 16)
                {
                    var current = feature;
                    string name = Read("IFeature.Name", () => current.Name, "Unknown feature");
                    string kind = Read("IFeature.GetTypeName2", () => current.GetTypeName2(), "Unknown");
                    if (seen.Add(name))
                    {
                        bool warning = false;
                        int error = Read("IFeature.GetErrorCode2", () => current.GetErrorCode2(out warning), -1);
                        var suppressed = Read("IFeature.IsSuppressed2", () => current.IsSuppressed2((int)swInConfigurationOpts_e.swThisConfiguration, null) as bool[]);
                        var f = new FeatureState { Name = name, Kind = kind, ErrorCode = error, Warning = warning,
                            ErrorMeaning = ErrorWords.Feature(error), Suppressed = suppressed?.FirstOrDefault() ?? false,
                            RolledBack = Read("IFeature.IsRolledBack", () => current.IsRolledBack()) };
                        if (kind == "Boss" || kind == "Extrusion")
                        {
                            var data = Read("IFeature.GetDefinition", () => current.GetDefinition() as IExtrudeFeatureData2);
                            if (data != null) f.MidPlane = Read("IExtrudeFeatureData2.GetEndCondition", () => data.GetEndCondition(true), -1) == (int)swEndConditions_e.swEndCondMidPlane;
                        }
                        s.Features.Add(f);
                        s.Rollback |= f.RolledBack;
                        if ((kind == "ProfileFeature" || kind == "3DProfileFeature") && !f.Suppressed && !f.RolledBack)
                        {
                            var sk = Read("IFeature.GetSpecificFeature2", () => current.GetSpecificFeature2() as ISketch);
                            if (sk != null) s.LastSketch = ReadSketch(sk, s, current);
                        }
                        Walk(Read("IFeature.GetFirstSubFeature", () => current.GetFirstSubFeature() as IFeature), true, depth + 1);
                    }
                    feature = sub ? Read("IFeature.GetNextSubFeature", () => current.GetNextSubFeature() as IFeature) : Read("IFeature.GetNextFeature", () => current.GetNextFeature() as IFeature);
                }
            }
            Walk(Read("IModelDoc2.FirstFeature", () => doc.FirstFeature() as IFeature), false, 0);
            if (count >= 1500) s.ReadIssues.Add("This tree is too large for a complete check.");
            var bodies = Read("IPartDoc.GetBodies2", () => ((IPartDoc)doc).GetBodies2((int)swBodyType_e.swSolidBody, false) as object[], null);
            s.BodyCount = bodies?.Length ?? 0;
            s.BodySignature = string.Join(";", (bodies ?? new object[0]).Select(b => {
                var box = Read("IBody2.GetBodyBox", () => ((IBody2)b).GetBodyBox() as double[]);
                return string.Join(",", (box ?? new double[0]).Select(v => v.ToString("G9", CultureInfo.InvariantCulture)));
            }));
            return s;
        }
        SketchState ReadSketch(ISketch sketch, StateSnapshot s, IFeature feature = null)
        {
            T Read<T>(string name, Func<T> action, T fallback = default(T)) => api.Get(name, action, fallback, s.ReadIssues);
            var sk = new SketchState();
            feature = feature ?? Read("ISketch.QueryInterface.IFeature", () => (IFeature)sketch);
            if (feature != null)
            {
                sk.Name = Read("IFeature.Name", () => feature.Name, "Sketch");
                bool warning = false;
                sk.ErrorCode = Read("IFeature.GetErrorCode2", () => feature.GetErrorCode2(out warning), -1);
                var dim = Read("IFeature.GetFirstDisplayDimension", () => feature.GetFirstDisplayDimension() as IDisplayDimension);
                var values = new List<string>();
                while (dim != null && values.Count < 200)
                {
                    var current = dim;
                    var d = Read("IDisplayDimension.GetDimension2", () => current.GetDimension2(0) as IDimension);
                    if (d != null) values.Add(Read("IDimension.FullName", () => d.FullName, "?") + "=" + Read("IDimension.SystemValue", () => d.SystemValue).ToString("R", CultureInfo.InvariantCulture));
                    dim = Read("IFeature.GetNextDisplayDimension", () => feature.GetNextDisplayDimension(current) as IDisplayDimension);
                }
                sk.DimensionCount = values.Count;
                sk.DimensionSignature = string.Join(";", values);
            }
            int type = 0;
            var support = Read("ISketch.GetReferenceEntity", () => sketch.GetReferenceEntity(ref type));
            if (support is IFeature plane)
            {
                sk.SupportKind = "plane";
                sk.Support = Read("IFeature.Name", () => plane.Name, "a plane");
            }
            else if (support is IFace2 face)
            {
                sk.SupportKind = "face";
                var owner = Read("IFace2.GetFeature", () => face.GetFeature() as IFeature);
                sk.Support = owner == null ? "a face" : "a face of " + Read("IFeature.Name", () => owner.Name, "the part");
            }
            sk.ConstraintCode = Read("ISketch.GetConstrainedStatus", () => sketch.GetConstrainedStatus(), -1);
            sk.ConstraintName = ErrorWords.ConstraintName(sk.ConstraintCode);
            sk.ConstraintMeaning = ErrorWords.Constraint(sk.ConstraintCode);
            var segments = Read("ISketch.GetSketchSegments", () => sketch.GetSketchSegments() as object[]) ?? new object[0];
            sk.EntityCount = segments.Count(o => !Read("ISketchSegment.ConstructionGeometry", () => ((ISketchSegment)o).ConstructionGeometry));
            var contours = Read("ISketch.GetSketchContours", () => sketch.GetSketchContours() as object[]) ?? new object[0];
            sk.ContourCount = contours.Length;
            foreach (ISketchContour contour in contours)
            {
                if (Read("ISketchContour.IsClosed", () => contour.IsClosed())) sk.ClosedContours++; else sk.OpenContours++;
            }
            sk.RegionCount = Read("ISketch.GetSketchRegionCount", () => sketch.GetSketchRegionCount(), -1);
            if (sk.EntityCount > 0)
            {
                int open=0, closed=0;
                sk.UsageCode = Read("ISketch.CheckFeatureUse", () => sketch.CheckFeatureUse((int)swSketchCheckFeatureProfileUsage_e.swSketchCheckFeature_BASEEXTRUDE, ref open, ref closed), -1);
                sk.UsageMeaning = ErrorWords.Contour(sk.UsageCode);
            }
            return sk;
        }
    }
}
