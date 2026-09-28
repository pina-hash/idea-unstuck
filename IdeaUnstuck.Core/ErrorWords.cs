using System;
using SolidWorks.Interop.swconst;

namespace IdeaUnstuck.Core
{
    public static class ErrorWords
    {
        public static string ConstraintName(int code) => Enum.GetName(typeof(swConstrainedStatus_e), code) ?? "Constraint code " + code;
        public static string Constraint(int code)
        {
            switch (code)
            {
                case 2: return "Your sketch can still move. Add a size with Smart Dimension.";
                case 3: return "Your sketch is fully defined. Its sizes and position are fixed.";
                case 4: return "Two sizes or drawing rules are fighting.";
                case 5: return "Your drawing rules cannot all be true at once.";
                case 6: return "SOLIDWORKS cannot solve these drawing rules.";
                case 7: return "Automatic Solve is off. Open Tools > Sketch Settings > Automatic Solve.";
                default: return ConstraintName(code) + ". Ask a TA.";
            }
        }
        public static string Feature(int code)
        {
            switch (code)
            {
                case 0: return "No error.";
                case 10: return "Fillet cannot find a complete chain of edges.";
                case 11: return "Fillet cannot find the chosen face.";
                case 12: return "The rounding size is not valid.";
                case 13: return "Fillet cannot find the chosen edge.";
                case 14: return "This edge cannot be rounded with these settings.";
                case 15: return "The rounding size is too small.";
                case 16: return "The nearby faces cannot reach the rounded edge.";
                case 17: return "The rounding would remove a needed face or edge.";
                case 18: case 19: return "The rounding size is too big for this edge.";
                case 30: return "The new shape does not touch the existing shape.";
                case 31: return "The extrusion cannot find its ending face.";
                case 32: return "These extrusion settings cannot make a solid shape.";
                case 33: case 36: return "The sketch mixes open and closed outlines.";
                case 34: case 35: case 37: return "The sketch outline cannot make this extrusion.";
                case 38: return "A rule joining parts points to an invalid edge.";
                case 39: return "A rule joining parts points to an invalid face.";
                case 40: return "A rule joining parts could not create a surface.";
                case 41: return "A rule joining parts points to an invalid item.";
                case 42: return "SOLIDWORKS cannot make the selected items touch smoothly.";
                case 43: case 48: return "A rule joining parts lost an item it needs.";
                case 44: return "This rule needs a straight item.";
                case 45: return "An item used to join parts failed.";
                case 46: return "Rules joining these parts are fighting.";
                case 47: return "A rule joining parts is not valid.";
                case 49: case 50: return "This feature uses an older tool. Ask a TA.";
                case 51: return "The sketch lost a link to another item.";
                case 52: return "The partial rounding does not meet its reference.";
                case 53: return "The partial rounding could not update.";
                case 54: return "The partial rounding cannot continue along these edges.";
                case 55: return "The partial rounding lost its starting edge.";
                case 56: return "The partial rounding lost its ending edge.";
                case 57: return "The partial rounding lost its main edge.";
                case 58: return "The partial rounding starts too far along the edge.";
                case 59: case 63: return "The partial rounding is missing a reference item.";
                case 60: return "The partial rounding has too many reference items.";
                case 61: return "The partial rounding distance is not valid.";
                case 62: return "The partial rounding start passes its end.";
                case 64: return "The partial rounding reference item is not valid.";
                case 65: return "These partial rounding settings are not supported.";
                case 66: return "Partial rounding cannot use this closed chain of edges.";
                case 67: return "The partial rounding reference matches more than one point.";
                case 68: return "SOLIDWORKS could not repair the partial rounding.";
                case 69: return "The sheet metal sweep needs a valid outline and path.";
                case 70: return "The sheet metal sweep crosses itself.";
                case 71: return "The cut does not touch the solid shape.";
                case 72: return "Lines in the sketch cross each other.";
                case 73: return "This feature is missing an item it needs.";
                default: return (Enum.GetName(typeof(swFeatureError_e), code) ?? "Feature code " + code) + ". Ask a TA.";
            }
        }
        public static string Contour(int code)
        {
            switch (code)
            {
                case 0: return "The outline fits this tool.";
                case 1: case 2: case 12: return "Lines cross each other. Remove the crossing.";
                case 4: return "More than two lines meet at one point.";
                case 5: return "The sketch has no drawing items.";
                case 6: case 9: case 14: case 18: return "The outline has open ends.";
                case 7: case 13: case 17: case 19: return "This tool needs one outline.";
                case 8: return "A line has no length. Delete it.";
                case 10: case 15: return "This tool needs an open outline.";
                case 11: return "The sketch mixes open and closed outlines.";
                case 16: return "The outlines are nested more than this tool allows.";
                case 20: return "This tool needs one open outline or separate closed outlines.";
                case 21: return "Choose a centerline for the shape to turn around.";
                case 22: return "The outline is open or cannot be followed.";
                case 23: return "The outline crosses its centerline.";
                default: return (Enum.GetName(typeof(swSketchCheckFeatureStatus_e), code) ?? "Contour code " + code) + ". Ask a TA.";
            }
        }
    }
}
