using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IdeaUnstuck.AddIn.Ui;
using IdeaUnstuck.Core;

namespace IdeaUnstuck.AddIn
{
    /// <summary>
    /// The task pane. Top to bottom: header (icon, name, text size), Help | Practice
    /// tabs, then the Help view: search box, live results, the "Right now" card,
    /// common problems and the full library. Opening an answer replaces the list
    /// in place. Rules this class keeps:
    ///  * It never takes keyboard focus on its own. Only a student's click or Tab
    ///    moves focus here; Esc in the search box hands it back to SOLIDWORKS.
    ///  * Render() (the 1.5 s poll) touches only the Right now card and the
    ///    practice ticks, and only when they changed. Search text, the open
    ///    answer, the selection and the scroll position are left alone.
    /// </summary>
    [ComVisible(true), Guid("73B7229D-3403-4476-B3D0-67B7A5781BA2"), ProgId("IdeaUnstuck.Pane"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public sealed class UnstuckPane : UserControl
    {
        /// <summary>
        /// Practice "Do it for me" is a small link under a step, so a student tries the
        /// step first. Set to false to go back to the v0.2 full-size button.
        /// </summary>
        public static bool DoItForMeIsSmallLink = true;

        public const string Placeholder = "Type what's happening, in your own words";
        Telemetry log;
        Action<string> action;
        IPaneSettings settings;
        readonly Skin skin;
        float scaleOverride, fontScale = 1f;
        readonly ToolTip tips = new ToolTip { InitialDelay = 800, ReshowDelay = 400, AutoPopDelay = 20000, ShowAlways = true };
        KnowledgeBase help;
        Glossary words;
        internal int HelpEntryCount => help?.Entries.Count ?? 0;

        // Fixed chrome
        readonly Panel header;
        readonly PictureBox logo;
        readonly Label title;
        readonly Tile[] sizeButtons = new Tile[3];
        readonly Panel tabBar;
        readonly Tile helpTab, practiceTab;
        // Scrolling body
        readonly Column body, helpView, practiceView;
        readonly Column message;
        readonly Words messageText;
        readonly Column welcome;
        readonly SearchBox search;
        readonly Column results, rightNow, noDocument, home, browse, answer;
        readonly Words rightNowText;
        readonly Tile rightNowButton;
        Tile browseToggle;
        readonly Tile export;
        readonly Dictionary<string, Tile> groupToggles = new Dictionary<string, Tile>();
        readonly Dictionary<string, Column> groupLists = new Dictionary<string, Column>();

        HelpEntry openEntry;
        readonly Stack<HelpEntry> history = new Stack<HelpEntry>();
        string cardKey = "", pathKey = "";
        Diagnosis cardDiagnosis;
        int documentType = -1;
        List<GuidedPath> paths;
        string openPath = "First 10 minutes";
        bool teacherMode;
        System.Speech.Synthesis.SpeechSynthesizer speech;

        public UnstuckPane() : this(new Telemetry(), id => { }) { }
        internal void Initialize(Telemetry logger, Action<string> handler) { log = logger; action = handler; }

        public UnstuckPane(Telemetry log, Action<string> action, IPaneSettings settings = null, float scale = 0, bool? highContrast = null)
        {
            this.log = log; this.action = action; this.settings = settings ?? new RegistryPaneSettings();
            scaleOverride = scale;
            AutoScaleDimensions = new SizeF(96f, 96f);
            AutoScaleMode = AutoScaleMode.Dpi;
            skin = new Skin(MakeTheme(highContrast));
            MinimumSize = new Size(200, 120);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Guard("help_load", () => help = KnowledgeBase.Embedded);
            Guard("words_load", () => words = Glossary.Embedded);

            // Body first, then the docked bars above it (WinForms docks in reverse z-order).
            body = new Column(skin, scrolls: true) { Dock = DockStyle.Fill, Gap = 10, Name = "Body" };
            Controls.Add(body);
            tabBar = new Panel { Dock = DockStyle.Top, Name = "Tabs" };
            helpTab = new Tile(skin, "Help", TileStyle.Tab) { Selected = true, Name = "HelpTab" };
            practiceTab = new Tile(skin, "Practice", TileStyle.Tab) { Name = "PracticeTab" };
            tabBar.Controls.Add(helpTab); tabBar.Controls.Add(practiceTab);
            tabBar.Layout += (s, e) => { int w = tabBar.Width / 2; helpTab.SetBounds(0, 0, w, tabBar.Height); practiceTab.SetBounds(w, 0, tabBar.Width - w, tabBar.Height); };
            helpTab.Click += (s, e) => Guard("tab_help", () => ShowTab(true));
            practiceTab.Click += (s, e) => Guard("tab_practice", () => ShowTab(false));
            Tip(helpTab, "Search for help and see what SOLIDWORKS is showing right now.");
            Tip(practiceTab, "Step-by-step practice parts. The boxes tick themselves when SOLIDWORKS shows the step is done.");
            Controls.Add(tabBar);
            header = new Panel { Dock = DockStyle.Top, Name = "Header" };
            logo = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, AccessibleName = "IDEA Unstuck icon", TabStop = false };
            Guard("icon_load", () => { using (var st = typeof(UnstuckPane).Assembly.GetManifestResourceStream("IdeaUnstuck.AddIn.icon-64.png")) if (st != null) logo.Image = Image.FromStream(st); });
            title = new Label { Text = "IDEA Unstuck", AutoSize = false, TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false, AccessibleName = "IDEA Unstuck" };
            title.Click += (s, e) => Guard("teacher_reveal", () => { if ((ModifierKeys & Keys.Control) != 0) { teacherMode = !teacherMode; export.Visible = teacherMode; Reflow(); } });
            header.Controls.Add(logo); header.Controls.Add(title);
            for (int i = 0; i < 3; i++)
            {
                int size = i;
                var b = new Tile(skin, Theme.TextSizeNames[i], TileStyle.HeaderToggle) { Name = "TextSize" + i };
                b.AccessibleName = new[] { "Normal text", "Bigger text", "Biggest text" }[i];
                b.Click += (s, e) => Guard("text_size", () => SetTextSize(size));
                Tip(b, new[] { "Normal text size.", "Bigger text.", "Biggest text." }[i] + " Remembered for your Windows account.");
                sizeButtons[i] = b; header.Controls.Add(b);
            }
            header.Layout += (s, e) => LayoutHeader();
            Controls.Add(header);

            // Help view
            helpView = new Column(skin) { Gap = 10, Name = "HelpView" };
            message = new Column(skin) { Visible = false, Padding = new Padding(10), Name = "Message" };
            messageText = new Words(skin, "");
            var messageOk = new Tile(skin, "OK", TileStyle.Link);
            messageOk.Click += (s, e) => Guard("message_ok", () => { message.Visible = false; Reflow(); });
            message.Controls.Add(messageText); message.Controls.Add(messageOk);
            welcome = new Column(skin) { Card = true, Padding = new Padding(10), Gap = 4, Name = "Welcome" };
            welcome.Controls.Add(new Words(skin, "Welcome to IDEA Unstuck", TextRole.Bold));
            welcome.Controls.Add(new Words(skin, "1. Stuck? Type what is happening in the box below, in your own words."));
            welcome.Controls.Add(new Words(skin, "2. Or pick one of the common problems."));
            welcome.Controls.Add(new Words(skin, "3. The Practice tab walks you through your first part."));
            var gotIt = new Tile(skin, "Got it", TileStyle.Secondary) { Name = "WelcomeOk" };
            gotIt.Click += (s, e) => Guard("welcome_ok", () => { this.settings.WelcomeDismissed = true; welcome.Visible = false; log.Write("button_pressed", new { action = "welcome_dismissed" }); Reflow(); });
            welcome.Controls.Add(gotIt);
            welcome.Visible = !this.settings.WelcomeDismissed;
            search = new SearchBox(skin, Placeholder) { Name = "SearchBox" };
            Tip(search.Box, "Type a few words, like: my lines are red. Press Enter to open the top answer. Press Esc to go back to SOLIDWORKS.");
            search.Box.TextChanged += (s, e) => Guard("help_search", SearchChanged);
            search.Box.KeyDown += (s, e) => Guard("search_key", () => SearchKey(e));
            search.Box.GotFocus += (s, e) => search.Invalidate();
            search.Box.LostFocus += (s, e) => search.Invalidate();
            results = new Column(skin) { Gap = 4, Visible = false, Name = "Results" };
            rightNow = new Column(skin) { Padding = new Padding(14, 10, 10, 10), Gap = 8, Visible = false, Name = "RightNow" };
            rightNow.Controls.Add(new Words(skin, "Right now", TextRole.SmallBold) { Soft = true });
            rightNowText = new Words(skin, "", TextRole.Bold) { Name = "RightNowText" };
            rightNowButton = new Tile(skin, "", TileStyle.Primary) { Name = "RightNowButton" };
            rightNowButton.Click += (s, e) => Guard("right_now", RightNowPressed);
            rightNow.Controls.Add(rightNowText); rightNow.Controls.Add(rightNowButton);
            noDocument = new Column(skin) { Padding = new Padding(14, 10, 10, 10), Gap = 8, Visible = false, Name = "NoDocument" };
            noDocument.Controls.Add(new Words(skin, "Open or start a part", TextRole.Bold));
            noDocument.Controls.Add(new Words(skin, "This helper reads your part while you work. Start a new part, or open yours with File > Open.") { Soft = true });
            var newPart = new Tile(skin, "New part", TileStyle.Primary) { Name = "NewPart" };
            newPart.Click += (s, e) => Guard("new_part", () => Act("newpart"));
            Tip(newPart, "Starts a new, empty part, the same as File > New > Part.");
            noDocument.Controls.Add(newPart);
            home = new Column(skin) { Gap = 4, Name = "Home" };
            browse = new Column(skin) { Gap = 4, Visible = false, Name = "Browse" };
            answer = new Column(skin) { Gap = 8, Visible = false, Name = "Answer" };
            export = new Tile(skin, "Teacher export", TileStyle.Secondary) { Visible = false, Name = "TeacherExport" };
            export.Click += (s, e) => Guard("export", () => Act("export"));
            Tip(export, "Saves today's local log to the Desktop as a ZIP. Nothing is sent anywhere.");
            BuildHome();
            foreach (Control c in new Control[] { message, welcome, search, results, rightNow, noDocument, home, answer, export }) helpView.Controls.Add(c);
            if (help == null) { search.Enabled = false; home.Controls.Clear(); home.Controls.Add(new Words(skin, "Help could not load. Raise your hand.")); }

            practiceView = new Column(skin) { Gap = 6, Visible = false, Name = "PracticeView" };
            body.Controls.Add(helpView); body.Controls.Add(practiceView);
            ApplyTheme();
            Resize += (s, e) => Guard("pane_resize", () => { LayoutHeader(); Reflow(); });
        }

        Theme MakeTheme(bool? highContrast = null)
        {
            float scale = scaleOverride > 0 ? scaleOverride : DeviceDpi / 96f;
            // In the harness the scale is simulated, so the fonts are scaled with it.
            fontScale = scaleOverride > 0 ? scaleOverride : 1f;
            return new Theme(scale, settings.TextSize, highContrast ?? skin?.T.HighContrast, fontScale);
        }

        void ApplyTheme()
        {
            var t = skin.T;
            BackColor = t.Page; body.BackColor = t.Page; helpView.BackColor = t.Page; practiceView.BackColor = t.Page;
            body.Padding = new Padding(t.Px(10), t.Px(10), t.Px(10), t.Px(14));
            header.BackColor = t.Header; title.ForeColor = t.HeaderInk; title.Font = t.HeaderFont; title.BackColor = t.Header;
            header.Height = t.Px(40);
            tabBar.Height = t.Px(38); tabBar.BackColor = t.Panel;
            message.BackColor = t.Message; welcome.BackColor = t.Row; rightNow.BackColor = t.Row; rightNow.Edge = t.CardEdge;
            noDocument.BackColor = t.Row; noDocument.Edge = t.CardEdge; answer.BackColor = t.Page;
            search.ApplyTheme();
            for (int i = 0; i < 3; i++) sizeButtons[i].Selected = t.TextSize == i;
            ForEachDeep(this, c => c.Invalidate());
            LayoutHeader();
            Reflow();
        }

        void LayoutHeader()
        {
            var t = skin.T; int h = header.Height; int pad = t.Px(8);
            int icon = t.Px(22);
            logo.SetBounds(pad, (h - icon) / 2, icon, icon);
            int bw = t.Px(34), bh = t.Px(26), gap = t.Px(3);
            int x = header.Width - pad - bw * 3 - gap * 2;
            bool room = x > pad + icon + t.Px(90);
            for (int i = 0; i < 3; i++) { sizeButtons[i].Visible = room || i == 0 || true; sizeButtons[i].SetBounds(x + i * (bw + gap), (h - bh) / 2, bw, bh); }
            title.SetBounds(pad + icon + t.Px(6), 0, Math.Max(10, x - (pad + icon + t.Px(6)) - gap), h);
        }

        void SetTextSize(int size)
        {
            settings.TextSize = size;
            log.Write("button_pressed", new { action = "text_size", size = Theme.TextSizeNames[size] });
            skin.T = MakeTheme();
            ApplyTheme();
        }

        /// <summary>Harness hook: same as the student pressing A, A+ or A++.</summary>
        internal void UseTextSize(int size) => SetTextSize(size);

        static void ForEachDeep(Control c, Action<Control> f) { foreach (Control child in c.Controls) { f(child); ForEachDeep(child, f); } }

        /// <summary>Lays out every nested stack from the inside out, keeping the reader's scroll position.</summary>
        void Reflow()
        {
            var keep = body.AutoScrollPosition;
            SuspendLayout();
            try
            {
                void Deep(Control c) { foreach (Control child in c.Controls) Deep(child); if (c is Column || c is Row || c is CheckRow) c.PerformLayout(); }
                Deep(body);
            }
            finally { ResumeLayout(true); }
            body.PerformLayout();
            body.AutoScrollPosition = new Point(0, -keep.Y);
            body.Invalidate(true);
        }

        void Guard(string name, Action call) { try { call(); } catch (Exception ex) { log?.Exception(name, ex); } }
        void Tip(Control c, string text) { if (!string.IsNullOrEmpty(text)) tips.SetToolTip(c, text); }
        void Act(string id) { try { action?.Invoke(id); } catch (Exception ex) { log.Exception("pane_action." + id, ex); } }

        // ---------- Help: home ----------
        void BuildHome()
        {
            if (help == null) return;
            home.Controls.Add(new Words(skin, "Common problems", TextRole.Bold) { Name = "CommonHeading" });
            foreach (var id in KnowledgeBase.CommonProblems) { var e = help.Find(id); if (e != null) home.Controls.Add(EntryTile(e, "common")); }
            browseToggle = new Tile(skin, "Browse all answers", TileStyle.Group) { Expander = true, Name = "BrowseAll" };
            browseToggle.Detail = help.Entries.Count + " answers, grouped by kind";
            browseToggle.Click += (s, e) => Guard("browse", () => {
                browse.Visible = !browse.Visible; browseToggle.Expanded = browse.Visible; browseToggle.UpdateName(); browseToggle.Invalidate();
                log.Write("button_pressed", new { action = "browse", open = browse.Visible }); Reflow(); });
            Tip(browseToggle, "Every answer in the library, in three groups.");
            home.Controls.Add(browseToggle);
            foreach (var kind in KnowledgeBase.Kinds)
            {
                var list = help.Entries.Where(x => x.Kind == kind).ToList();
                var toggle = new Tile(skin, KnowledgeBase.KindLabel(kind) + " (" + list.Count + ")", TileStyle.Group) { Expander = true, Name = "Group_" + kind };
                var box = new Column(skin) { Gap = 4, Visible = false, Name = "GroupList_" + kind };
                foreach (var e in list) box.Controls.Add(EntryTile(e, "browse", showTag: false));
                toggle.Click += (s, e) => Guard("group", () => { box.Visible = !box.Visible; toggle.Expanded = box.Visible; toggle.UpdateName(); toggle.Invalidate(); Reflow(); });
                groupToggles[kind] = toggle; groupLists[kind] = box;
                browse.Controls.Add(toggle); browse.Controls.Add(box);
            }
            home.Controls.Add(browse);
        }

        Tile EntryTile(HelpEntry e, string source, bool showTag = true)
        {
            var tile = new Tile(skin, e.Title, TileStyle.Row) { Name = "Entry_" + e.Id };
            if (showTag) { tile.Tag2 = KnowledgeBase.KindLabel(e.Kind); tile.TagKind = e.Kind; }
            tile.UpdateName();
            Tip(tile, e.Fix);
            tile.Click += (s, ev) => Guard("open_entry", () => OpenEntry(e, source));
            return tile;
        }

        // ---------- Help: search ----------
        IReadOnlyList<HelpResult> shown = new HelpResult[0];
        void SearchChanged()
        {
            if (help == null) return;
            var q = search.Box.Text;
            if (openEntry != null) CloseAnswer(false);
            ClearChildren(results);
            if (string.IsNullOrWhiteSpace(q)) { results.Visible = false; home.Visible = true; shown = new HelpResult[0]; Reflow(); return; }
            var watch = Stopwatch.StartNew();
            shown = help.Search(q);
            results.Visible = true; home.Visible = false;
            if (shown.Count == 0)
            {
                results.Controls.Add(new Words(skin, "No answer matched those words. Try one short word you can see on the screen, like sketch, extrude, red, tree or units. Or pick a common problem below.") { Name = "NoMatch" });
                var back = new Tile(skin, "Show common problems", TileStyle.Link) { Name = "ShowCommon" };
                back.Click += (s, e) => Guard("show_common", () => { home.Visible = true; Reflow(); });
                results.Controls.Add(back);
            }
            else foreach (var r in shown) results.Controls.Add(EntryTile(r.Entry, "search"));
            LastSearchMs = watch.Elapsed.TotalMilliseconds;
            Reflow();
            LastSearchMs = watch.Elapsed.TotalMilliseconds;
        }
        internal double LastSearchMs { get; private set; }

        void SearchKey(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = e.SuppressKeyPress = true;
                if (shown.Count > 0) OpenEntry(shown[0].Entry, "enter");
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = e.SuppressKeyPress = true;
                search.Box.Text = "";
                log.Write("button_pressed", new { action = "escape_to_solidworks" });
                Act("focus-sw");
            }
        }

        // ---------- Help: answer ----------
        void OpenEntry(HelpEntry e, string source)
        {
            if (openEntry != null && openEntry != e) history.Push(openEntry);
            openEntry = e;
            log.Write("help_opened", new { id = e.Id, source, query = source == "search" || source == "enter" ? search.Box.Text : null });
            StopSpeech();
            ClearChildren(answer);
            var t = skin.T;
            var back = new Tile(skin, history.Count > 0 ? "< Back" : string.IsNullOrWhiteSpace(search.Box.Text) ? "< Back to the list" : "< Back to results", TileStyle.Link) { Name = "Back" };
            back.Click += (s, ev) => Guard("answer_back", GoBack);
            Tip(back, "Go back.");
            answer.Controls.Add(back);
            answer.Controls.Add(new Words(skin, KnowledgeBase.KindLabel(e.Kind), TextRole.SmallBold) { Soft = true });
            answer.Controls.Add(new Words(skin, e.Title, TextRole.Title) { Name = "AnswerTitle" });
            var fix = new Words(skin, e.Fix, TextRole.Bold) { Name = "AnswerFix" };
            Tip(fix, words?.Tip(e.Fix));
            answer.Controls.Add(fix);
            var steps = new Column(skin) { Gap = 8, Name = "Steps" };
            for (int i = 0; i < e.Steps.Count; i++)
            {
                var st = e.Steps[i];
                var view = new StepView(skin, i + 1, st.T, WhereTag(st.W)) { Name = "Step" + (i + 1) };
                var tip = "Where: " + WhereLong(st.W) + "." + (words?.Tip(st.T) is string g ? "\r\n\r\n" + g : "");
                Tip(view, tip);
                steps.Controls.Add(view);
            }
            if (e.Steps.Count > 0) answer.Controls.Add(steps); else steps.Dispose();
            var doIt = RescueFor(e.Id);
            if (doIt != null)
            {
                var link = new Tile(skin, "Tried the steps? " + doIt.Item2, TileStyle.Link) { Name = "AnswerDoIt" };
                link.Click += (s, ev) => Guard("answer_do_it", () => { log.Write("do_it_for_me", new { source = "answer", id = e.Id, action = doIt.Item1 }); Act(doIt.Item1); });
                Tip(link, "The helper does this one step in SOLIDWORKS for you.");
                answer.Controls.Add(link);
            }
            var why = new Words(skin, "Why: " + e.Why) { Soft = true, Name = "AnswerWhy" };
            Tip(why, words?.Tip(e.Why));
            answer.Controls.Add(why);
            answer.Controls.Add(new Words(skin, "Did that fix it?", TextRole.Bold) { Name = "FixedQuestion" });
            var row = new Row(skin) { Name = "FixedRow" };
            var yes = new Tile(skin, "Yes", TileStyle.Success) { Name = "FixedYes" };
            var no = new Tile(skin, "Not yet", TileStyle.Secondary) { Name = "FixedNo" };
            row.Controls.Add(yes); row.Controls.Add(no);
            answer.Controls.Add(row);
            var after = new Column(skin) { Gap = 4, Visible = false, Name = "AfterFeedback" };
            answer.Controls.Add(after);
            yes.Click += (s, ev) => Guard("fixed_yes", () => {
                log.Write("help_feedback", new { id = e.Id, fixedIt = true });
                ClearChildren(after); after.Controls.Add(new Words(skin, "Nice work. Keep going.", TextRole.Bold)); after.Visible = true; Reflow(); });
            no.Click += (s, ev) => Guard("fixed_no", () => {
                log.Write("help_feedback", new { id = e.Id, fixedIt = false });
                ClearChildren(after);
                if (e.Related.Count > 0) after.Controls.Add(new Words(skin, "Try one of these:", TextRole.Bold));
                foreach (var id in e.Related) { var r = help.Find(id); if (r != null) after.Controls.Add(EntryTile(r, "related")); }
                after.Controls.Add(new Words(skin, "Still stuck? Raise your hand.", TextRole.Bold) { Name = "RaiseHand" });
                after.Visible = true; Reflow(); });
            Tip(yes, "Tell the helper this answer worked.");
            Tip(no, "Shows answers to related problems.");
            var read = new Tile(skin, "Read it to me", TileStyle.Secondary) { Name = "ReadAloud" };
            read.Click += (s, ev) => Guard("read_aloud", () => ReadAloud(e, read));
            Tip(read, "Reads this answer out loud. Press again to stop.");
            answer.Controls.Add(read);
            var copy = new Tile(skin, "Copy for AI", TileStyle.Link) { Name = "CopyForAi" };
            copy.Click += (s, ev) => Guard("copy_ai", () => Act("copy:" + e.Id));
            Tip(copy, "Copies a description of your part and this problem, so you can paste it into an AI chat if you choose. Nothing is sent.");
            answer.Controls.Add(copy);
            results.Visible = false; home.Visible = false; rightNow.Visible = false; noDocument.Visible = false; welcome.Visible = false;
            answer.Visible = true;
            Reflow();
            body.AutoScrollPosition = new Point(0, 0);
        }

        static Tuple<string, string> RescueFor(string id)
        {
            switch (id)
            {
                case "lost-part": return Tuple.Create("find", "Find my part");
                case "units-mm": return Tuple.Create("units", "Set units to inches");
                case "left-sketch": return Tuple.Create("edit", "Get back into my sketch");
                case "rollback": return Tuple.Create("roll", "Roll to end");
                default: return null;
            }
        }

        void GoBack()
        {
            if (history.Count > 0) { var prev = history.Pop(); openEntry = null; OpenEntry(prev, "back"); return; }
            CloseAnswer(true);
        }

        void CloseAnswer(bool relayout)
        {
            StopSpeech();
            openEntry = null; history.Clear();
            ClearChildren(answer); answer.Visible = false;
            bool typing = !string.IsNullOrWhiteSpace(search.Box.Text);
            results.Visible = typing; home.Visible = !typing;
            welcome.Visible = !settings.WelcomeDismissed;
            ShowCard();
            if (relayout) Reflow();
        }

        void ReadAloud(HelpEntry e, Tile button)
        {
            if (speech != null && speech.State == System.Speech.Synthesis.SynthesizerState.Speaking) { StopSpeech(); button.Text = "Read it to me"; return; }
            if (speech == null) { speech = new System.Speech.Synthesis.SpeechSynthesizer(); speech.Rate = -1; speech.SpeakCompleted += (s, a) => Guard("speech_done", () => { if (!button.IsDisposed) button.Text = "Read it to me"; }); }
            var text = e.Title + ". " + e.Fix + " " + string.Join(" ", e.Steps.Select((st, i) => "Step " + (i + 1) + ". " + st.T)) + (e.Steps.Count == 0 ? " " + e.Why : "");
            log.Write("button_pressed", new { action = "read_aloud", id = e.Id });
            button.Text = "Stop reading";
            speech.SpeakAsync(text);
        }
        void StopSpeech() { try { speech?.SpeakAsyncCancelAll(); } catch (Exception ex) { log?.Exception("speech_stop", ex); } }

        static string WhereTag(string w)
        {
            switch (w)
            {
                case "tree": case "property": case "rollback": return "Left panel";
                case "ribbon": return "Top ribbon";
                case "tabs": return "Top tabs";
                case "menu": return "Top menu";
                case "units": return "Bottom-right corner";
                case "status": return "Bottom bar";
                case "corner": return "Top-right corner";
                case "headsup": return "Above the part";
                case "graphics": return "Middle of the screen";
                case "taskpane": return "Right panel";
                case "keyboard": return "Keyboard";
                case "mouse": return "Mouse";
                default: return "";
            }
        }
        static string WhereLong(string w)
        {
            switch (w)
            {
                case "menu": return "the menus at the very top"; case "tabs": return "the tabs at the top (Features, Sketch, Evaluate)";
                case "ribbon": return "the tool buttons under the tabs"; case "tree": return "the tree on the left";
                case "property": return "the settings panel on the left, with the green check"; case "rollback": return "the yellow and black bar in the tree";
                case "graphics": return "the big part area"; case "headsup": return "the small view tools above the part";
                case "corner": return "the top-right corner of the part area"; case "taskpane": return "the panel on the far right";
                case "status": return "the bar along the bottom"; case "units": return "the bottom-right corner";
                case "keyboard": return "your keyboard"; case "mouse": return "your mouse";
                default: return "the SOLIDWORKS window";
            }
        }

        // ---------- Right now card ----------
        static readonly HashSet<string> CardActions = new HashSet<string> { "units", "edit", "roll", "undo", "front" };
        static readonly HashSet<string> CardIds = new HashSet<string> { "wrong-document", "still-drawing", "tree-gone", "wrong-select", "rebuild" };
        /// <summary>The one diagnosis confident enough to show, or null. Stuck, read gaps and reminders never show here.</summary>
        public static Diagnosis Confident(IEnumerable<Diagnosis> list) => list?.FirstOrDefault(d => d.Id != "stuck" && d.Id != "read-incomplete" && d.Id != "unsaved" &&
            (d.IsError || (d.Action != null && CardActions.Contains(d.Action)) || CardIds.Contains(d.Id)));
        static string ActionLabel(string a) => a == "units" ? "Set units to inches" : a == "roll" ? "Roll to end" : a == "undo" ? "Undo" : a == "front" ? "Start a sketch on the Front Plane" : a == "edit" ? "Get back into my sketch" : null;

        void ShowCard()
        {
            bool answerOpen = openEntry != null;
            noDocument.Visible = !answerOpen && documentType == 0;
            rightNow.Visible = !answerOpen && documentType != 0 && cardDiagnosis != null;
        }

        void RightNowPressed()
        {
            var d = cardDiagnosis; if (d == null) return;
            log.Write("right_now_pressed", new { d.Id, d.Action });
            if (d.Action != null && CardActions.Contains(d.Action)) Act(d.Action);
            else { var e = help?.ForDiagnosis(d.Id); if (e != null) OpenEntry(e, "right_now"); }
        }

        public void ShowMessage(string text)
        {
            messageText.Text = text; message.Visible = true;
            if (!helpView.Visible) ShowTab(true);
            Reflow();
        }

        /// <summary>
        /// Called by the 1.5 s poll. Changes only the card and the practice ticks,
        /// and only when they changed, so nothing moves under a reading student.
        /// </summary>
        public void Render(StateSnapshot state, List<Diagnosis> diagnoses, List<GuidedPath> guided)
        {
            var watch = Stopwatch.StartNew();
            bool changed = false;
            int type = state?.DocumentType ?? 0;
            var d = type == 1 && (state.ReadIssues?.Count ?? 0) == 0 ? Confident(diagnoses) : type > 1 ? diagnoses?.FirstOrDefault(x => x.Id == "wrong-document") : null;
            string key = type + "|" + (d == null ? "" : d.Id + "|" + d.Title + "|" + d.Action);
            if (key != cardKey)
            {
                cardKey = key; documentType = type; cardDiagnosis = d; changed = true;
                if (d != null)
                {
                    rightNowText.Text = d.Title;
                    var label = d.Action != null ? ActionLabel(d.Action) : null;
                    rightNowButton.Text = label ?? "Show me how";
                    rightNowButton.UpdateName();
                    tips.SetToolTip(rightNowText, d.Meaning + " " + d.NextClick);
                    tips.SetToolTip(rightNowButton, label != null ? "The helper does this for you in SOLIDWORKS: " + label + "." : "Opens the steps for this problem.");
                }
                ShowCard();
            }
            string pk = guided == null ? "" : string.Join("|", guided.Select(p => p.Title + ":" + string.Concat(p.Steps.Select(s => s.Done ? "1" : "0"))));
            if (pk != pathKey) { pathKey = pk; paths = guided; BuildPractice(); changed = true; }
            if (changed) Reflow();
            LastRenderMs = watch.Elapsed.TotalMilliseconds;
            RenderCount++;
            if (changed) ChangedRenders++;
        }
        internal double LastRenderMs { get; private set; }
        internal int RenderCount { get; private set; }
        internal int ChangedRenders { get; private set; }

        // ---------- Practice ----------
        void ShowTab(bool helpTabShown)
        {
            helpView.Visible = helpTabShown; practiceView.Visible = !helpTabShown;
            helpTab.Selected = helpTabShown; practiceTab.Selected = !helpTabShown;
            helpTab.UpdateName(); practiceTab.UpdateName(); helpTab.Invalidate(); practiceTab.Invalidate();
            log.Write("button_pressed", new { action = helpTabShown ? "tab_help" : "tab_practice" });
            Reflow();
            body.AutoScrollPosition = new Point(0, 0);
        }
        /// <summary>Harness hook: same as clicking the tab.</summary>
        internal void ShowPractice(bool practice) => ShowTab(!practice);

        void BuildPractice()
        {
            ClearChildren(practiceView);
            practiceView.Controls.Add(new Words(skin, "Practice parts", TextRole.Bold));
            practiceView.Controls.Add(new Words(skin, "Open one path. Do each step in SOLIDWORKS. Each box ticks itself when SOLIDWORKS shows the step is done.") { Soft = true });
            if (paths == null) return;
            foreach (var path in paths.OrderByDescending(p => p.Title == "First 10 minutes"))
            {
                bool open = path.Title == openPath;
                int done = path.Steps.Count(s => s.Done);
                var head = new Tile(skin, path.Title, TileStyle.Group) { Expander = true, Expanded = open, Detail = done + " of " + path.Steps.Count + " done", Name = "Path_" + path.Title.Replace(' ', '_') };
                head.UpdateName();
                var p = path;
                head.Click += (s, e) => Guard("path_expand", () => {
                    openPath = openPath == p.Title ? null : p.Title;
                    log.Write("button_pressed", new { action = "path_toggle", path = p.Title, open = openPath == p.Title });
                    BuildPractice(); Reflow(); });
                Tip(head, open ? "Close this path." : "Open this path. Only one path is open at a time.");
                practiceView.Controls.Add(head);
                if (!open) continue;
                var list = new Column(skin) { Gap = 10, Padding = new Padding(skin.T.Px(4), skin.T.Px(4), 0, skin.T.Px(8)) };
                foreach (var step in path.Steps)
                {
                    var row = new CheckRow(skin, step.Text, step.Done) { Name = "Step_" + step.Id };
                    var st = step;
                    row.Box.Click += (s, e) => Guard("check_click", () => ShowMessage(st.Done ? "Done. SOLIDWORKS shows this step is finished." : "This box ticks itself when SOLIDWORKS shows the step is done. Do the step in SOLIDWORKS."));
                    Tip(row.Box, "Ticks itself when SOLIDWORKS shows the step is done.");
                    Tip(row.Label, words?.Tip(step.Text));
                    if (step.Hint != null)
                    {
                        var hint = new Words(skin, step.Hint) { Soft = true, Visible = false };
                        var where = new Tile(skin, "Show me where", TileStyle.Link);
                        where.Click += (s, e) => Guard("show_where", () => { hint.Visible = !hint.Visible; where.Text = hint.Visible ? "Hide where" : "Show me where"; log.Write("button_pressed", new { action = "show_where", step = st.Id, open = hint.Visible }); Reflow(); });
                        Tip(where, "Shows where on the SOLIDWORKS screen to look.");
                        row.AddLink(where); row.AddLink(hint);
                    }
                    if (!step.Done && step.Action != null)
                    {
                        var doIt = new Tile(skin, "Do it for me", DoItForMeIsSmallLink ? TileStyle.Link : TileStyle.Secondary);
                        doIt.Click += (s, e) => Guard("do_it_for_me", () => { log.Write("do_it_for_me", new { source = "practice", step = st.Id, action = st.Action, path = p.Title }); Act(st.Action); });
                        Tip(doIt, "Try the step yourself first. This does it in SOLIDWORKS for you.");
                        row.AddLink(doIt);
                    }
                    list.Controls.Add(row);
                }
                practiceView.Controls.Add(list);
            }
        }

        void ClearChildren(Control parent)
        {
            while (parent.Controls.Count > 0)
            {
                var c = parent.Controls[0];
                ForEachDeep(c, x => tips.SetToolTip(x, null)); tips.SetToolTip(c, null);
                parent.Controls.Remove(c); c.Dispose();
            }
        }

        protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); Guard("dpi_check", RescaleIfNeeded); }
        protected override void OnDpiChangedAfterParent(EventArgs e) { base.OnDpiChangedAfterParent(e); Guard("dpi_change", RescaleIfNeeded); }
        void RescaleIfNeeded()
        {
            if (scaleOverride > 0 || Math.Abs(skin.T.Scale - DeviceDpi / 96f) < 0.01f) return;
            skin.T = MakeTheme(); ApplyTheme();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { try { speech?.Dispose(); } catch { } tips.Dispose(); }
            base.Dispose(disposing);
        }

        // ---------- Harness and test hooks (internal: not part of the COM surface) ----------
        internal Control Find(string name) => Controls.Find(name, true).FirstOrDefault();
        internal void TypeSearch(string text) => search.Box.Text = text;
        internal void PressEnter() => SearchKey(new KeyEventArgs(Keys.Enter));
        internal void PressEscape() => SearchKey(new KeyEventArgs(Keys.Escape));
        internal HelpEntry OpenAnswer => openEntry;
        internal string SearchText => search.Box.Text;
        internal Point ScrollPosition { get => body.AutoScrollPosition; set => body.AutoScrollPosition = value; }
        internal bool HorizontalScrollShown => body.HorizontalScroll.Visible;
        internal int BodyContentHeight => body.DisplayRectangle.Height;
        internal Control BodyPanel => body;
        internal void OpenEntryById(string id) { var e = help?.Find(id); if (e != null) OpenEntry(e, "harness"); }
        internal void SetWelcome(bool show) { welcome.Visible = show && openEntry == null; Reflow(); }
    }
}
