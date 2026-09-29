// Compares IdeaUnstuck.Core.SearchEngine with the post's own JavaScript, whose
// answers tools/search-oracle/oracle.js wrote to expected.json (top five ids and
// scores for 772 queries). Exit 0 only when every query matches exactly and the
// 22 pinned titles come first.
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;
using IdeaUnstuck.Core;

static class P
{
    static int Main(string[] args)
    {
        var root = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
        var kb = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "design", "kb-v2.json"))).RootElement;
        var entries = kb.GetProperty("entries").EnumerateArray().Select(e => new HelpEntry {
            Id = e.GetProperty("id").GetString(), Kind = e.GetProperty("kind").GetString(), Title = e.GetProperty("title").GetString(),
            Q = e.GetProperty("q").GetString(), Fix = e.GetProperty("fix").GetString(), Why = e.GetProperty("why").GetString(),
            Steps = e.GetProperty("steps").EnumerateArray().Select(s => new HelpStep { T = s.GetProperty("t").GetString(), W = s.GetProperty("w").GetString() }).ToList(),
            Related = e.GetProperty("related").EnumerateArray().Select(r => r.GetString()).ToList() }).ToList();
        var syn = kb.GetProperty("syn").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetString());
        var engine = new SearchEngine(entries, syn);
        int bad = 0, n = 0;
        foreach (var c in JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tools", "search-oracle", "expected.json"))).RootElement.EnumerateArray())
        {
            n++;
            var q = c.GetProperty("q").GetString();
            var want = string.Join(" | ", c.GetProperty("top").EnumerateArray().Select(t => t[0].GetString() + "=" + t[1].GetDouble().ToString("0.######")));
            var got = string.Join(" | ", engine.Search(q).Select(r => r.Entry.Id + "=" + Math.Round(r.Score, 6).ToString("0.######")));
            if (want != got) { bad++; Console.WriteLine("DIFF " + JsonSerializer.Serialize(q) + "\n  js: " + want + "\n  cs: " + got); }
        }
        var pins = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tools", "search-oracle", "pins.json"))).RootElement.EnumerateArray().ToList();
        int pinBad = 0;
        foreach (var p in pins)
        {
            var q = p[0].GetString(); var want = p[1].GetString();
            var top = engine.Search(q).FirstOrDefault()?.Entry.Title ?? "(nothing)";
            Console.WriteLine((top == want ? "PASS " : "FAIL ") + q + " -> " + top);
            if (top != want) pinBad++;
        }
        var none = engine.Search("zzqx blorp").Count;
        Console.WriteLine((none == 0 ? "PASS" : "FAIL") + " zzqx blorp -> " + none + " results");
        Console.WriteLine($"SEARCHCHECK queries={n} differences={bad} pins={pins.Count} pinFailures={pinBad}");
        return bad == 0 && pinBad == 0 && none == 0 && n > 700 ? 0 : 1;
    }
}
