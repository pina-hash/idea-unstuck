using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace IdeaUnstuck.Core
{
    /// <summary>
    /// SOLIDWORKS words and their plain meanings, from the SolidWorks Day post
    /// (IdeaUnstuck.Core/gloss.json). Used for hover help on answer steps.
    /// </summary>
    public sealed class Glossary
    {
        static readonly Lazy<Glossary> embedded = new Lazy<Glossary>(() => {
            using (var stream = typeof(Glossary).Assembly.GetManifestResourceStream("IdeaUnstuck.Core.gloss.json"))
            {
                if (stream == null) throw new InvalidDataException("The embedded word list is missing.");
                using (var reader = new StreamReader(stream)) return new Glossary(new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(reader.ReadToEnd()));
            }
        });
        public static Glossary Embedded => embedded.Value;
        // The post's Front Plane line points at a picture of a wrench on that page.
        static readonly Dictionary<string, string> Overrides = new Dictionary<string, string> { ["Front Plane"] = "The flat sheet you look straight at." };
        readonly List<KeyValuePair<Regex, KeyValuePair<string, string>>> terms;
        public int Count => terms.Count;
        public Glossary(IDictionary<string, string> words)
        {
            terms = words.OrderByDescending(p => p.Key.Length).Select(p => {
                var meaning = Overrides.TryGetValue(p.Key, out var o) ? o : p.Value;
                bool lower = p.Key == p.Key.ToLowerInvariant() && char.IsLetter(p.Key[0]);
                var pattern = @"(?<![\w.+])" + Regex.Escape(p.Key) + (char.IsLetterOrDigit(p.Key[p.Key.Length - 1]) ? @"(?!\w)" : "");
                var regex = new Regex(pattern, (lower ? RegexOptions.IgnoreCase : RegexOptions.None) | RegexOptions.CultureInvariant);
                return new KeyValuePair<Regex, KeyValuePair<string, string>>(regex, new KeyValuePair<string, string>(p.Key, meaning));
            }).ToList();
        }
        /// <summary>Terms found in <paramref name="text"/>, longest names first, a term inside a longer one skipped.</summary>
        public IList<KeyValuePair<string, string>> TermsIn(string text, int max = 3)
        {
            var found = new List<KeyValuePair<string, string>>();
            var taken = new bool[(text ?? "").Length];
            foreach (var t in terms)
            {
                if (found.Count >= max) break;
                foreach (Match m in t.Key.Matches(text ?? ""))
                {
                    if (Enumerable.Range(m.Index, m.Length).Any(i => taken[i])) continue;
                    for (int i = m.Index; i < m.Index + m.Length; i++) taken[i] = true;
                    found.Add(t.Value);
                    break;
                }
            }
            return found;
        }
        public string Tip(string text)
        {
            var list = TermsIn(text);
            return list.Count == 0 ? null : string.Join("\r\n", list.Select(p => p.Key + ": " + p.Value));
        }
    }
}
