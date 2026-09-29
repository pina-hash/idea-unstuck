using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace IdeaUnstuck.Core
{
    public sealed class HelpDocument
    {
        public int Version { get; set; }
        public List<HelpEntry> Entries { get; set; }
        public Dictionary<string, string> Syn { get; set; }
        public List<List<string>> Keys { get; set; }
    }
    /// <summary>
    /// The 99-entry library shared with the SolidWorks Day post (design/kb-v2.json,
    /// embedded in this DLL so help works with no network and no data file).
    /// </summary>
    public sealed class KnowledgeBase
    {
        public const string ResourceName = "IdeaUnstuck.Core.kb-v2.json";
        public static readonly string[] Kinds = { "wrong", "how", "what" };
        public static string KindLabel(string kind) => kind == "wrong" ? "Something is wrong" : kind == "how" ? "How do I" : kind == "what" ? "What is" : "";
        /// <summary>The six rows under "Common problems": the traps first-day students hit most.</summary>
        public static readonly string[] CommonProblems = { "open-contour", "red-sketch", "left-sketch", "lost-part", "units-mm", "tree-gone" };

        static readonly Lazy<KnowledgeBase> embedded = new Lazy<KnowledgeBase>(() => {
            using (var stream = typeof(KnowledgeBase).Assembly.GetManifestResourceStream(ResourceName))
            {
                if (stream == null) throw new InvalidDataException("The embedded help file is missing.");
                using (var reader = new StreamReader(stream)) return FromJson(reader.ReadToEnd());
            }
        });
        readonly Dictionary<string, HelpEntry> byId;
        readonly SearchEngine engine;
        public static KnowledgeBase Embedded => embedded.Value;
        public IReadOnlyList<HelpEntry> Entries { get; }
        public IReadOnlyDictionary<string, string> Synonyms { get; }
        public IReadOnlyList<KeyValuePair<string, string>> Keys { get; }

        KnowledgeBase(HelpDocument document)
        {
            if (document == null || document.Version != 2 || document.Entries == null || document.Entries.Count == 0)
                throw new InvalidDataException("The help file is empty or has an unsupported version.");
            byId = new Dictionary<string, HelpEntry>(StringComparer.Ordinal);
            foreach (var entry in document.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || byId.ContainsKey(entry.Id) ||
                    !Kinds.Contains(entry.Kind) || string.IsNullOrWhiteSpace(entry.Title) ||
                    string.IsNullOrWhiteSpace(entry.Q) || string.IsNullOrWhiteSpace(entry.Fix) ||
                    string.IsNullOrWhiteSpace(entry.Why) || entry.Steps == null || entry.Steps.Count == 0 ||
                    entry.Steps.Any(s => s == null || string.IsNullOrWhiteSpace(s.T) || string.IsNullOrWhiteSpace(s.W)) || entry.Related == null)
                    throw new InvalidDataException("The help file has an incomplete or duplicate entry: " + entry?.Id);
                byId.Add(entry.Id, entry);
            }
            foreach (var entry in document.Entries)
                foreach (var related in entry.Related)
                    if (related == null || !byId.ContainsKey(related)) throw new InvalidDataException("Unknown related help: " + related);
            foreach (var target in DiagnosisLinks.Values.Concat(CommonProblems))
                if (!byId.ContainsKey(target)) throw new InvalidDataException("Unknown diagnosis help: " + target);
            Entries = document.Entries.AsReadOnly();
            var syn = new Dictionary<string, string>(document.Syn ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            Synonyms = new ReadOnlyDictionary<string, string>(syn);
            Keys = (document.Keys ?? new List<List<string>>()).Where(k => k != null && k.Count == 2).Select(k => new KeyValuePair<string, string>(k[0], k[1])).ToList().AsReadOnly();
            engine = new SearchEngine(Entries, syn);
        }
        public static KnowledgeBase FromJson(string json) => new KnowledgeBase(new JavaScriptSerializer { MaxJsonLength = 16 * 1024 * 1024 }.Deserialize<HelpDocument>(json));
        public HelpEntry Find(string id) => id != null && byId.TryGetValue(id, out var entry) ? entry : null;
        public IReadOnlyList<HelpResult> Search(string query) => engine.Search(query);

        public static IReadOnlyDictionary<string, string> DiagnosisLinks { get; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal) {
            ["no-document"] = "new-part", ["wrong-document"] = "new-part", ["read-incomplete"] = "rebuild", ["other-units"] = "units-mm", ["stuck"] = "check-sketch",
            ["still-drawing"] = "still-drawing", ["wrong-plane"] = "wrong-plane", ["no-dimensions"] = "smart-dim", ["radius-not-dia"] = "radius-not-dia", ["extrude-direction"] = "extrude-direction", ["rebuild"] = "rebuild", ["no-sketch"] = "start-sketch",
            ["tree-gone"] = "tree-gone", ["wrong-select"] = "wrong-select",
            ["open-contour"] = "open-contour", ["multiple-outlines"] = "multi-contour", ["sketch-error"] = "multi-contour",
            ["left-sketch"] = "left-sketch", ["millimeters"] = "units-mm", ["over-defined"] = "red-sketch",
            ["feature"] = "red-x", ["rollback"] = "rollback", ["no-body"] = "extrude",
            ["multiple-bodies"] = "two-bodies", ["unsaved"] = "save", ["lost-part"] = "lost-part"
        });
        public HelpEntry ForDiagnosis(string id)
        {
            if (id == null) return null;
            var key = id.Split(':')[0];
            return DiagnosisLinks.TryGetValue(key, out var target) ? Find(target) : null;
        }
    }
}
