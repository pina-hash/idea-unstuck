using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using IdeaUnstuck.Core;

namespace IdeaUnstuck.AddIn
{
    public sealed class UnstuckPane : UserControl
    {
        readonly Telemetry log;
        readonly Action<string> action;
        readonly FlowLayoutPanel root = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(8) };
        readonly Label status, message;
        readonly FlowLayoutPanel cards, checklist;
        readonly Button export;
        readonly HashSet<string> expanded = new HashSet<string> { "My first 3D part" };
        string fingerprint;
        List<GuidedPath> paths;
        public UnstuckPane(Telemetry log, Action<string> action)
        {
            this.log = log; this.action = action;
            Font = new Font("Segoe UI", 10); ForeColor = Color.FromArgb(20, 30, 45); BackColor = Color.White;
            AutoScaleMode = AutoScaleMode.Dpi; MinimumSize = new Size(180, 100);
            Controls.Add(root);
            var title = TextLabel("IDEA Unstuck", true); title.Font = new Font(Font.FontFamily, 15, FontStyle.Bold);
            title.Click += (s, e) => Guard("teacher_reveal", () => { if ((ModifierKeys & Keys.Control) != 0) export.Visible = !export.Visible; });
            root.Controls.Add(title);
            status = TextLabel(""); root.Controls.Add(status);
            message = TextLabel(""); message.BackColor = Color.FromArgb(225, 242, 255); message.Visible = false; root.Controls.Add(message);
            cards = Stack(); root.Controls.Add(cards);
            root.Controls.Add(TextLabel("One next click", true));
            foreach (var item in new[] { ("find", "Find my part"), ("units", "Set units to inches"), ("edit", "Get back into my sketch"), ("exit", "Exit sketch"), ("roll", "Roll to end"), ("undo", "Undo"), ("front", "Start a sketch on the Front Plane"), ("copy", "Copy for AI") }) root.Controls.Add(ActionButton(item.Item2, item.Item1));
            root.Controls.Add(TextLabel("Practice paths", true));
            checklist = Stack(); root.Controls.Add(checklist);
            export = ActionButton("Teacher export", "export"); export.Visible = false; root.Controls.Add(export);
            Resize += (s, e) => Guard("pane_resize", FitWidths);
            FitWidths();
        }
        FlowLayoutPanel Stack() => new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(0, 6, 0, 6) };
        Label TextLabel(string text, bool bold = false) => new Label { Text = text, AutoSize = true, Padding = new Padding(4), Margin = new Padding(0, 3, 0, 3), Font = new Font(Font, bold ? FontStyle.Bold : FontStyle.Regular), UseMnemonic = false };
        Button ActionButton(string text, string id)
        {
            var button = new Button { Text = text, MinimumSize = new Size(0, 40), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(5, 7, 5, 7), Margin = new Padding(0, 3, 0, 3), BackColor = Color.FromArgb(232, 238, 246), ForeColor = ForeColor, FlatStyle = FlatStyle.Flat, AccessibleName = text, UseMnemonic = false };
            button.Click += (s, e) => Guard("pane_button." + id, () => action(id));
            return button;
        }
        void Guard(string name, Action call) { try { call(); } catch (Exception ex) { log.Exception(name, ex); } }
        public void ShowMessage(string text) { message.Text = text; message.Visible = true; FitWidths(); }
        public void Render(StateSnapshot state, List<Diagnosis> diagnoses, List<GuidedPath> guided)
        {
            string next = state.StatusLine + string.Join("|", diagnoses.Select(d => d.ToString())) + string.Join("", guided.SelectMany(p => p.Steps).Select(s => s.Done ? "1" : "0"));
            if (next == fingerprint) return;
            fingerprint = next; paths = guided;
            SuspendLayout();
            try
            {
                status.Text = state.StatusLine;
                Clear(cards);
                foreach (var d in diagnoses)
                {
                    var card = Stack(); card.Padding = new Padding(6); card.BackColor = d.IsError ? Color.FromArgb(255, 236, 225) : Color.FromArgb(240, 244, 248);
                    card.Controls.Add(TextLabel(d.Title, true)); card.Controls.Add(TextLabel(d.Meaning)); card.Controls.Add(TextLabel(d.NextClick));
                    if (d.Action != null) card.Controls.Add(ActionButton(d.Action == "copy" ? "Copy for AI" : d.Action == "units" ? "Set units to inches" : d.Action == "roll" ? "Roll to end" : d.Action == "undo" ? "Undo" : "Get back into my sketch", d.Action));
                    cards.Controls.Add(card);
                }
                RenderPaths(); FitWidths();
            }
            finally { ResumeLayout(true); }
        }
        void RenderPaths()
        {
            Clear(checklist);
            if (paths == null) return;
            foreach (var path in paths)
            {
                var heading = new Button { Text = (expanded.Contains(path.Title) ? "Hide: " : "Show: ") + path.Title, AutoSize = true, MinimumSize = new Size(0, 40), Margin = new Padding(0, 4, 0, 4), FlatStyle = FlatStyle.Flat, UseMnemonic = false };
                heading.Click += (s, e) => Guard("path_expand", () => { if (!expanded.Add(path.Title)) expanded.Remove(path.Title); RenderPaths(); FitWidths(); });
                checklist.Controls.Add(heading);
                if (!expanded.Contains(path.Title)) continue;
                foreach (var step in path.Steps)
                {
                    checklist.Controls.Add(TextLabel((step.Done ? "✓ " : "□ ") + step.Text));
                    if (!step.Done && step.Action != null) checklist.Controls.Add(ActionButton("Do it for me", step.Action));
                }
            }
        }
        static void Clear(Control parent) { while (parent.Controls.Count > 0) { var control = parent.Controls[0]; parent.Controls.Remove(control); control.Dispose(); } }
        void FitWidths()
        {
            int width = Math.Max(150, ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 22);
            void Fit(Control parent, int available)
            {
                foreach (Control child in parent.Controls)
                {
                    child.Width = available;
                    child.MaximumSize = new Size(available, 0);
                    if (child is FlowLayoutPanel) Fit(child, Math.Max(120, available - child.Padding.Horizontal));
                }
            }
            Fit(root, width);
        }
    }
}
