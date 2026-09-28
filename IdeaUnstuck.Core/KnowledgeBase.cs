using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace IdeaUnstuck.Core
{
    public sealed class HelpStep
    {
        public string T { get; set; }
        public string W { get; set; }
    }
    public sealed class HelpEntry
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public string Q { get; set; }
        public string Fix { get; set; }
        public List<HelpStep> Steps { get; set; }
        public string Why { get; set; }
        public List<string> Related { get; set; }
    }
    public sealed class HelpDocument
    {
        public int Version { get; set; }
        public List<HelpEntry> Entries { get; set; }
    }
    public sealed class HelpResult
    {
        public HelpEntry Entry { get; internal set; }
        public double Score { get; internal set; }
    }
    public sealed class KnowledgeBase
    {
        sealed class IndexedEntry
        {
            public HelpEntry Entry;
            public string[] Words, Phrases;
        }
        static readonly Lazy<KnowledgeBase> embedded = new Lazy<KnowledgeBase>(() => {
            using (var stream = typeof(KnowledgeBase).Assembly.GetManifestResourceStream("IdeaUnstuck.Core.kb.json"))
            {
                if (stream == null) throw new InvalidDataException("The embedded help file is missing.");
                using (var reader = new StreamReader(stream)) return FromJson(reader.ReadToEnd());
            }
        });
        readonly Dictionary<string, HelpEntry> byId;
        readonly IndexedEntry[] index;
        public static KnowledgeBase Embedded => embedded.Value;
        public IReadOnlyList<HelpEntry> Entries { get; }

        KnowledgeBase(HelpDocument document)
        {
            if (document == null || document.Version != 1 || document.Entries == null || document.Entries.Count == 0)
                throw new InvalidDataException("The help file is empty or has an unsupported version.");
            byId = new Dictionary<string, HelpEntry>(StringComparer.Ordinal);
            foreach (var entry in document.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Id) || byId.ContainsKey(entry.Id) ||
                    (entry.Kind != "wrong" && entry.Kind != "how") || string.IsNullOrWhiteSpace(entry.Title) ||
                    string.IsNullOrWhiteSpace(entry.Q) || string.IsNullOrWhiteSpace(entry.Fix) ||
                    string.IsNullOrWhiteSpace(entry.Why) || entry.Steps == null || entry.Steps.Count == 0 ||
                    entry.Steps.Any(s => s == null || string.IsNullOrWhiteSpace(s.T) || string.IsNullOrWhiteSpace(s.W)) || entry.Related == null)
                    throw new InvalidDataException("The help file has an incomplete or duplicate entry.");
                byId.Add(entry.Id, entry);
            }
            foreach (var entry in document.Entries)
                foreach (var related in entry.Related)
                    if (related == null || !byId.ContainsKey(related)) throw new InvalidDataException("Unknown related help: " + related);
            foreach (var target in DiagnosisLinks.Values)
                if (!byId.ContainsKey(target)) throw new InvalidDataException("Unknown diagnosis help: " + target);
            Entries = document.Entries.AsReadOnly();
            index = document.Entries.Select(e => new IndexedEntry {
                Entry = e,
                Words = Words(e.Title + " " + e.Q).Distinct(StringComparer.Ordinal).ToArray(),
                Phrases = e.Q.Split(';').Select(Normalize).Where(p => p.Length > 0).Distinct(StringComparer.Ordinal).ToArray()
            }).ToArray();
        }
        public static KnowledgeBase FromJson(string json) => new KnowledgeBase(new JavaScriptSerializer().Deserialize<HelpDocument>(json));
        public HelpEntry Find(string id) => id != null && byId.TryGetValue(id, out var entry) ? entry : null;
        static string Normalize(string value) => string.Join(" ", Words(value));
        static string[] Words(string value) => Regex.Matches((value ?? "").ToLowerInvariant().Replace("'", "").Replace("’", ""), @"[\p{L}\p{Nd}]+")
            .Cast<Match>().Select(m => m.Value).ToArray();

        public IReadOnlyList<HelpResult> Search(string query)
        {
            var normalized = Normalize(query);
            if (normalized.Length == 0) return new HelpResult[0];
            var words = normalized.Split(' ').Distinct(StringComparer.Ordinal).ToArray();
            var phraseQuery = " " + normalized + " ";
            // One best score per distinct query word; OR matching. LINQ's stable
            // OrderBy preserves the supplied entry order when scores are equal.
            return index.Select(e => new HelpResult {
                Entry = e.Entry,
                Score = words.Sum(q => e.Words.Max(w => WordScore(q, w))) +
                    4 * e.Phrases.Count(p => phraseQuery.Contains(" " + p + " "))
            }).Where(r => r.Score > 0).OrderByDescending(r => r.Score).Take(5).ToArray();
        }
        static double WordScore(string query, string word)
        {
            if (query == word) return 3;
            if (word.StartsWith(query, StringComparison.Ordinal)) return 2;
            return query.Length >= 4 && word.Length >= 4 && query.All(char.IsLetter) && word.All(char.IsLetter) && OneTypo(query, word) ? 1.5 : 0;
        }
        // Distance exactly one: insertion, deletion, substitution, or adjacent
        // transposition. Linear work suffices because larger distances never score.
        static bool OneTypo(string a, string b)
        {
            if (Math.Abs(a.Length - b.Length) > 1) return false;
            int first = 0;
            while (first < Math.Min(a.Length, b.Length) && a[first] == b[first]) first++;
            if (a.Length != b.Length)
            {
                var longer = a.Length > b.Length ? a : b;
                var shorter = a.Length > b.Length ? b : a;
                return longer.Substring(first + 1) == shorter.Substring(first);
            }
            if (first == a.Length) return false;
            return a.Substring(first + 1) == b.Substring(first + 1) ||
                (first + 1 < a.Length && a[first] == b[first + 1] && a[first + 1] == b[first] && a.Substring(first + 2) == b.Substring(first + 2));
        }

        public static IReadOnlyDictionary<string, string> DiagnosisLinks { get; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal) {
            ["no-document"] = "new-part", ["wrong-document"] = "new-part", ["read-incomplete"] = "rebuild", ["other-units"] = "units-mm", ["stuck"] = "check-sketch",
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
