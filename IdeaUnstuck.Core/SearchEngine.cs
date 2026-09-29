using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IdeaUnstuck.Core
{
    /// <summary>
    /// C# port of the Unstuck search in the SolidWorks Day post
    /// (design/reference/IDEA100_D04_SolidWorks-Day.html, the block starting
    /// "search: any word counts"). The add-in and the post must answer a question
    /// the same way, so this follows the JavaScript line by line, including its
    /// quirks (repeated query words count twice, a phrase listed twice in an
    /// entry earns its bonus twice, dl() answers 2 whenever the lengths differ by
    /// more than one). tools/search-oracle runs the original JavaScript and
    /// SelfTest --unit compares the two.
    /// Pure BCL: no SOLIDWORKS, no JSON, no UI.
    /// </summary>
    public sealed class SearchEngine
    {
        public const double Threshold = 2;
        public const int MaxResults = 5;

        static readonly HashSet<string> Stop = new HashSet<string>(("i my me the a an it is are to and of in on how do does what why can with for at be this that so just its im you your was were has have had get got there here when where which who will would should could up out off into from by as or if then than too very really make let thing things like super little kinda sorta please help").Split(' '), StringComparer.Ordinal);
        static readonly Regex Problem = new Regex(@"\b(gone|missing|disappear\w*|wont|cant|cannot|error|red|yellow|broke\w*|not|stuck|wrong|weird|failed|fail\w*|lost|help|crash\w*|froze|frozen|nothing|grey|gray)\b", RegexOptions.CultureInvariant);
        static readonly Regex WhatWord = new Regex(@"\bwhat\b|\bmean", RegexOptions.CultureInvariant);

        sealed class Indexed
        {
            public HelpEntry Entry;
            public string[] Tokens;
            public string[] Phrases;
        }

        readonly Indexed[] index;
        readonly IDictionary<string, string> synonyms;

        public SearchEngine(IEnumerable<HelpEntry> entries, IDictionary<string, string> synonyms)
        {
            this.synonyms = synonyms ?? new Dictionary<string, string>();
            index = entries.Select(e => new Indexed {
                Entry = e,
                Tokens = Toks(e.Title + " " + e.Q.Replace(";", " ")).Select(Stem).ToArray(),
                Phrases = e.Q.Split(';').Select(p => " " + Norm(p).Trim(' ') + " ").ToArray()
            }).ToArray();
        }

        // norm(): lower case, fold the common contractions, keep a-z 0-9 + and space.
        public static string Norm(string s)
        {
            var t = (s ?? "").ToLowerInvariant();
            t = Regex.Replace(t, "can't|cannot|can not", "cant");
            t = Regex.Replace(t, "won't|will not", "wont");
            t = Regex.Replace(t, "don't|do not", "dont");
            t = Regex.Replace(t, "doesn't|does not", "doesnt");
            var b = new StringBuilder(t.Length);
            foreach (var c in t) b.Append((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '+' || c == ' ' ? c : ' ');
            return b.ToString();
        }

        public static string[] Toks(string s) => Regex.Split(Norm(s), @"\s+").Where(w => w.Length > 0 && !Stop.Contains(w)).ToArray();

        // stem(): words longer than four letters lose one ending. The JS regex
        // (ing|ed|es|s)$ matches at the leftmost position, which is the longest ending.
        public static string Stem(string w)
        {
            if (w.Length <= 4) return w;
            if (w.EndsWith("ing", StringComparison.Ordinal)) return w.Substring(0, w.Length - 3);
            if (w.EndsWith("ed", StringComparison.Ordinal) || w.EndsWith("es", StringComparison.Ordinal)) return w.Substring(0, w.Length - 2);
            if (w.EndsWith("s", StringComparison.Ordinal)) return w.Substring(0, w.Length - 1);
            return w;
        }

        // Optimal string alignment (restricted Damerau-Levenshtein). Answers 2 early
        // when the lengths differ by more than one, exactly as the post does.
        public static int Dl(string a, string b)
        {
            if (Math.Abs(a.Length - b.Length) > 1) return 2;
            var d = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) d[0, j] = j;
            for (int i = 1; i <= a.Length; i++)
                for (int j = 1; j <= b.Length; j++)
                {
                    int c = a[i - 1] == b[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + c);
                    if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1]) d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + 1);
                }
            return d[a.Length, b.Length];
        }

        static double WordScore(string w, Indexed x)
        {
            double best = 0; var sw = Stem(w);
            foreach (var t in x.Tokens)
            {
                if (t == sw) best = Math.Max(best, 3);
                else if (sw.Length >= 3 && (t.StartsWith(sw, StringComparison.Ordinal) || (sw.StartsWith(t, StringComparison.Ordinal) && t.Length >= 5))) best = Math.Max(best, 2);
                else if (w.Length >= 5 && t.Length >= 5 && (Dl(w, t) <= 1 || Dl(sw, t) <= 1)) best = Math.Max(best, 2.5);
                else if (sw.Length >= 4 && Dl(sw, t) <= 1) best = Math.Max(best, 1.5);
                else if (sw.Length >= 7 && Dl(sw, t) <= 2) best = Math.Max(best, 1);
            }
            return best;
        }

        double Score(Indexed x, string[] qt, string qn)
        {
            double s = 0;
            foreach (var w in qt)
            {
                var b = WordScore(w, x);
                string syn;
                if (synonyms.TryGetValue(w, out syn) || synonyms.TryGetValue(Stem(w), out syn))
                    foreach (var sw in Toks(syn)) b = Math.Max(b, WordScore(sw, x) * 0.85);
                s += b;
            }
            foreach (var p in x.Phrases) if (p.Length > 4 && qn.IndexOf(p, StringComparison.Ordinal) >= 0) s += 4;
            if (x.Entry.Kind == "wrong" && Problem.IsMatch(qn)) s += 1.5;
            if (x.Entry.Kind == "what" && WhatWord.IsMatch(qn)) s += 1.5;
            return s;
        }

        /// <summary>Entries scoring at least 2, best first, ties in library order, at most <paramref name="max"/>.</summary>
        public IReadOnlyList<HelpResult> Search(string query, int max = MaxResults)
        {
            var qt = Toks(query);
            if (qt.Length == 0) return new HelpResult[0];
            var qn = " " + Regex.Replace(Norm(query), @"\s+", " ") + " ";
            // LINQ OrderByDescending is stable, like Array.prototype.sort in current engines.
            return index.Select(x => new HelpResult { Entry = x.Entry, Score = Score(x, qt, qn) })
                .Where(r => r.Score >= Threshold)
                .OrderByDescending(r => r.Score)
                .Take(max)
                .ToArray();
        }
    }
}
