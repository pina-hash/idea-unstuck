using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using IdeaUnstuck.AddIn;
using IdeaUnstuck.Core;

namespace PaneHarness
{
    /// <summary>
    /// PaneHarness.exe [output folder] [--show]
    /// Draws the real task pane in seven states at pane widths 260, 320 and 420 and
    /// scales 100, 125 and 150 percent (the scale is simulated: layout and fonts are
    /// multiplied, which is what a 125 or 150 percent Windows setting does to this
    /// pane), times polls and searches, checks focus and horizontal scrolling, and
    /// writes PNGs plus harness-report.json. Exit 0 only when every check passes.
    /// No SOLIDWORKS is involved: the states are made up here.
    /// </summary>
    static class Program
    {
        static readonly List<string> failures = new List<string>();
        static readonly Telemetry log = new Telemetry(Path.Combine(Path.GetTempPath(), "IdeaUnstuckHarnessLogs"));

        static StateSnapshot NoDoc() => new StateSnapshot { DocumentType = 0 };
        static StateSnapshot Mm() => new StateSnapshot { DocumentType = 1, DocumentKey = "h", FileName = "SMITH_PART.SLDPRT", LengthUnit = 0, UnitSystem = 1, BodyCount = 0, InSketch = true, Sketch = new SketchState { Name = "Sketch1", Support = "Front Plane", SupportKind = "plane", EntityCount = 1, FullCircleCount = 1, ClosedContours = 1 }, Path = "C:\\x.SLDPRT" };
        static StateSnapshot Clean() => new StateSnapshot { DocumentType = 1, DocumentKey = "h", FileName = "SMITH_PART.SLDPRT", LengthUnit = 3, UnitSystem = 3, BodyCount = 1, Path = "C:\\x.SLDPRT" };

        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var outDir = args.FirstOrDefault(a => !a.StartsWith("--")) ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pane-shots");
            Directory.CreateDirectory(outDir);
            if (args.Contains("--show")) { Show(); return 0; }
            var report = new Dictionary<string, object>();
            var shots = new List<object>();
            foreach (var scale in new[] { 1f, 1.25f, 1.5f })
                foreach (var width in new[] { 260, 320, 420 })
                    foreach (var state in new[] { "first-run", "empty", "typing", "answer", "right-now", "practice", "no-document" })
                        shots.Add(Shot(outDir, state, width, scale, false));
            shots.Add(Shot(outDir, "answer", 320, 1f, true));
            shots.Add(Shot(outDir, "right-now", 320, 1f, true));
            shots.Add(Shot(outDir, "answer-a++", 320, 1f, false));
            report["shots"] = shots;
            report["timing"] = Timing();
            report["focus"] = FocusCheck();
            report["failures"] = failures;
            File.WriteAllText(Path.Combine(outDir, "harness-report.json"), new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(report));
            Console.WriteLine("HARNESS shots=" + shots.Count + " failures=" + failures.Count);
            foreach (var f in failures) Console.WriteLine("FAIL " + f);
            return failures.Count == 0 ? 0 : 1;
        }

        static UnstuckPane Make(float scale, bool firstRun, bool? highContrast = null) =>
            new UnstuckPane(log, id => Console.WriteLine("action " + id), new MemoryPaneSettings { WelcomeDismissed = !firstRun }, scale, highContrast) { Dock = DockStyle.Fill };

        static object Shot(string outDir, string state, int width, float scale, bool highContrast)
        {
            int w = (int)Math.Round(width * scale), h = (int)Math.Round(820 * scale);
            // The runner's screen is 1024x768, and Windows clips a window to the screen,
            // so the pane is sized on its own (taller than its small host window) and
            // drawn directly. The host window only provides a window handle.
            using (var form = new Form { FormBorderStyle = FormBorderStyle.None, StartPosition = FormStartPosition.Manual, Location = new Point(0, 0), ClientSize = new Size(Math.Min(w, 600), 300), ShowInTaskbar = false })
            using (var pane = Make(scale, state == "first-run", highContrast ? true : (bool?)null))
            {
                pane.Dock = DockStyle.None; pane.Location = new Point(0, 0); pane.Size = new Size(w, h);
                form.Controls.Add(pane); form.Show(); form.ActiveControl = null; Application.DoEvents();
                var paths = new GuidedPaths(log).Paths;
                switch (state)
                {
                    case "first-run": case "empty": pane.Render(Clean(), new Rules().Evaluate(Clean()), paths); break;
                    case "typing": pane.Render(Clean(), new Rules().Evaluate(Clean()), paths); pane.Find("HelpSearch").Focus(); pane.TypeSearch("my lines are re"); break;
                    case "answer": case "answer-a++":
                        pane.Render(Mm(), new Rules().Evaluate(Mm()), paths); pane.TypeSearch("it wont let me extrude"); pane.PressEnter(); form.ActiveControl = null;
                        if (state == "answer-a++") pane.UseTextSize(2);
                        break;
                    case "right-now": pane.Render(Mm(), new Rules().Evaluate(Mm()), paths); break;
                    case "practice": { var g = new GuidedPaths(log); var s = Mm(); s.LengthUnit = 3; s.UnitSystem = 3; g.Update(s); pane.Render(s, new Rules().Evaluate(s), g.Paths); pane.ShowPractice(true); break; }
                    case "no-document": pane.Render(NoDoc(), new Rules().Evaluate(NoDoc()), paths); break;
                }
                Application.DoEvents();
                var name = state + "-w" + width + "-s" + (int)(scale * 100) + (highContrast ? "-hc" : "") + ".png";
                using (var bmp = new Bitmap(w, h))
                {
                    pane.DrawToBitmap(bmp, new Rectangle(0, 0, w, h));
                    bmp.Save(Path.Combine(outDir, name), ImageFormat.Png);
                }
                bool hscroll = pane.HorizontalScrollShown;
                var wide = Overflow(pane);
                if (hscroll) failures.Add(name + ": horizontal scrollbar shown");
                if (wide.Count > 0) failures.Add(name + ": controls wider than the pane: " + string.Join(", ", wide.Take(5)));
                var small = TooSmall(pane, scale);
                if (small.Count > 0) failures.Add(name + ": buttons under 36 px tall: " + string.Join(", ", small.Take(5)));
                form.Hide();
                return new { name, width, scale, highContrast, horizontalScroll = hscroll, overflow = wide.Count, shortButtons = small.Count, contentHeight = pane.BodyContentHeight };
            }
        }

        static IEnumerable<Control> All(Control c) => c.Controls.Cast<Control>().SelectMany(x => new[] { x }.Concat(All(x)));

        static List<string> Overflow(UnstuckPane pane)
        {
            var body = pane.BodyPanel; var bad = new List<string>();
            int limit = body.ClientSize.Width;
            foreach (var c in All(body).Where(x => x.Visible))
            {
                var r = body.RectangleToClient(c.RectangleToScreen(c.ClientRectangle));
                if (r.Right > limit + 1 || r.Left < -1) bad.Add(c.GetType().Name + ":" + c.Name + ":" + c.Text + " right=" + r.Right + " limit=" + limit);
            }
            return bad;
        }
        static List<string> TooSmall(UnstuckPane pane, float scale)
        {
            return All(pane.BodyPanel).OfType<IdeaUnstuck.AddIn.Ui.Tile>()
                .Where(t => t.Visible && t.Style != IdeaUnstuck.AddIn.Ui.TileStyle.Link && t.Height < Math.Round(36 * scale) - 1)
                .Select(t => t.Text + "=" + t.Height).ToList();
        }

        static double P95(List<double> xs) { xs.Sort(); return xs.Count == 0 ? 0 : xs[(int)Math.Ceiling(xs.Count * 0.95) - 1]; }

        static object Timing()
        {
            using (var form = new Form { ClientSize = new Size(320, 520), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(0, 0) })
            using (var pane = Make(1f, false))
            {
                form.Controls.Add(pane); form.Show(); Application.DoEvents();
                var paths = new GuidedPaths(log).Paths;
                // Warm up every path once (first calls pay for .NET compiling the code), then measure.
                pane.Render(Mm(), new Rules().Evaluate(Mm()), paths); pane.Render(Clean(), new Rules().Evaluate(Clean()), paths);
                pane.TypeSearch("warm up extrude"); pane.TypeSearch(""); pane.OpenEntryById("fillet"); pane.TypeSearch("x"); pane.TypeSearch(""); Application.DoEvents();
                var same = new List<double>(); var changedRenders = new List<double>(); var searches = new List<double>(); var opens = new List<double>();
                for (int i = 0; i < 300; i++) { var s = Mm(); var sw = Stopwatch.StartNew(); pane.Render(s, new Rules().Evaluate(s), paths); same.Add(sw.Elapsed.TotalMilliseconds); }
                for (int i = 0; i < 60; i++) { var s = i % 2 == 0 ? Mm() : Clean(); var sw = Stopwatch.StartNew(); pane.Render(s, new Rules().Evaluate(s), paths); Application.DoEvents(); changedRenders.Add(sw.Elapsed.TotalMilliseconds); }
                var queries = new[] { "it wont let me extrude", "my lines are red", "i lost my part", "how do i make a hole", "the list on the left is gone", "make it thicker", "hexagon", "zzqx blorp", "what is a plane", "turn in my part" };
                for (int r = 0; r < 5; r++) foreach (var q in queries) { var sw = Stopwatch.StartNew(); pane.TypeSearch(q); Application.DoEvents(); searches.Add(sw.Elapsed.TotalMilliseconds); }
                pane.TypeSearch("");
                var ids = new[] { "open-contour", "red-sketch", "lost-part", "units-mm", "tree-gone", "fillet", "cut", "attach-file", "what-plane", "polygon" };
                for (int r = 0; r < 4; r++) foreach (var id in ids) { var sw = Stopwatch.StartNew(); pane.OpenEntryById(id); Application.DoEvents(); opens.Add(sw.Elapsed.TotalMilliseconds); }
                // A reader in the middle of a long answer is not moved by polling.
                pane.OpenEntryById("open-contour"); Application.DoEvents();
                pane.ScrollPosition = new Point(0, 120); Application.DoEvents();
                var before = pane.ScrollPosition;
                if (before.Y == 0) failures.Add("scroll check could not scroll the answer, so it proves nothing");
                for (int i = 0; i < 10; i++) { var s = i % 2 == 0 ? Mm() : Clean(); pane.Render(s, new Rules().Evaluate(s), paths); Application.DoEvents(); }
                var after = pane.ScrollPosition;
                if (before != after) failures.Add("polling moved the scroll position from " + before + " to " + after);
                if (pane.OpenAnswer?.Id != "open-contour") failures.Add("polling closed the open answer");
                var result = new { unchangedPollP95Ms = P95(same), changedPollP95Ms = P95(changedRenders), searchKeystrokeP95Ms = P95(searches), openAnswerP95Ms = P95(opens), scrollKept = before == after, scrollBefore = -before.Y, scrollAfter = -after.Y, samples = new { polls = same.Count, changed = changedRenders.Count, searches = searches.Count, opens = opens.Count } };
                foreach (var pair in new[] { ("unchanged poll", P95(same)), ("changed poll", P95(changedRenders)), ("search", P95(searches)), ("open answer", P95(opens)) })
                {
                    Console.WriteLine("p95 " + pair.Item1 + " = " + pair.Item2.ToString("0.0") + " ms");
                    if (pair.Item2 > 50) failures.Add(pair.Item1 + " p95 " + pair.Item2.ToString("0.0") + " ms is over 50 ms");
                }
                return result;
            }
        }

        static object FocusCheck()
        {
            using (var form = new Form { ClientSize = new Size(700, 820), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(0, 0) })
            using (var pane = Make(1f, true))
            {
                var stand = new TextBox { Dock = DockStyle.Left, Width = 300, Name = "SolidWorksStandIn" };
                pane.Dock = DockStyle.Right; pane.Width = 320;
                form.Controls.Add(stand); form.Controls.Add(pane); form.Show(); form.Activate(); stand.Focus(); Application.DoEvents();
                bool start = stand.Focused;
                var paths = new GuidedPaths(log);
                for (int i = 0; i < 20; i++) { var s = i % 3 == 0 ? Mm() : i % 3 == 1 ? Clean() : NoDoc(); paths.Update(s); pane.Render(s, new Rules().Evaluate(s), paths.Paths); Application.DoEvents(); }
                bool afterPolls = stand.Focused && !pane.ContainsFocus;
                // Student clicks into the search box, types, then presses Esc.
                var box = (TextBox)pane.Find("HelpSearch"); box.Focus(); pane.TypeSearch("my lines are red"); Application.DoEvents();
                bool typed = box.Focused;
                pane.PressEscape(); Application.DoEvents();
                bool cleared = pane.SearchText == "";
                if (!start) failures.Add("focus: stand-in could not be focused (check is inconclusive)");
                if (!afterPolls) failures.Add("focus: polling moved focus into the pane");
                if (!typed) failures.Add("focus: search box did not keep focus while typing");
                if (!cleared) failures.Add("focus: Esc did not clear the search box");
                return new { standInFocusedAtStart = start, focusKeptThroughPolls = afterPolls, searchKeptFocusWhileTyping = typed, escClearedSearch = cleared, escSendsFocusToSolidWorks = "action focus-sw requested; the real handoff needs SOLIDWORKS" };
            }
        }

        static void Show()
        {
            var form = new Form { Text = "IDEA Unstuck pane harness", ClientSize = new Size(340, 900) };
            var pane = Make(0, true); form.Controls.Add(pane);
            var s = Mm(); pane.Render(s, new Rules().Evaluate(s), new GuidedPaths(log).Paths);
            Application.Run(form);
        }
    }
}
