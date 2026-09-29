using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace IdeaUnstuck.AddIn.Ui
{
    /// <summary>Shared, swappable theme so a text-size change reaches every control.</summary>
    public sealed class Skin
    {
        public Theme T { get; set; }
        public Skin(Theme t) { T = t; }
    }

    /// <summary>A control whose height follows from the width it is given.</summary>
    public interface IMeasured { int MeasureHeight(int width); }

    public enum TextRole { Body, Bold, Small, SmallBold, Title }

    static class Text
    {
        public const TextFormatFlags Wrap = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
        public static Font Of(Theme t, TextRole r) => r == TextRole.Bold ? t.Bold : r == TextRole.Small ? t.Small : r == TextRole.SmallBold ? t.SmallBold : r == TextRole.Title ? t.Title : t.Body;
        public static int Height(string s, Font f, int width)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            return TextRenderer.MeasureText(s, f, new Size(Math.Max(10, width), int.MaxValue), Wrap).Height;
        }
    }

    /// <summary>Wrapped text drawn by us, so measuring and painting use one set of rules.</summary>
    public sealed class Words : Control, IMeasured
    {
        readonly Skin skin;
        public TextRole Role { get; set; }
        public bool Soft { get; set; }
        public Words(Skin skin, string text, TextRole role = TextRole.Body)
        {
            this.skin = skin; Role = role; Text = text;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false; BackColor = Color.Transparent;
            AccessibleRole = AccessibleRole.StaticText;
        }
        protected override void OnTextChanged(EventArgs e) { AccessibleName = Text; base.OnTextChanged(e); Invalidate(); }
        string cacheKey; int cacheH;
        public int MeasureHeight(int width)
        {
            if (Text.Length == 0) return 0;
            var font = global::IdeaUnstuck.AddIn.Ui.Text.Of(skin.T, Role);
            var key = width + "|" + font.GetHashCode() + "|" + skin.T.Scale + "|" + Text;
            if (key == cacheKey) return cacheH;
            cacheH = global::IdeaUnstuck.AddIn.Ui.Text.Height(Text, font, width) + skin.T.Px(2); cacheKey = key; return cacheH;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var t = skin.T;
            TextRenderer.DrawText(e.Graphics, Text, global::IdeaUnstuck.AddIn.Ui.Text.Of(t, Role), new Rectangle(0, 0, Width, Height + 4), Soft ? t.InkSoft : t.Ink, global::IdeaUnstuck.AddIn.Ui.Text.Wrap);
        }
    }

    public enum TileStyle { Primary, Success, Secondary, Row, Link, Tab, HeaderToggle, Group }

    /// <summary>
    /// A button we draw ourselves: full width, text that wraps, an optional kind
    /// tag ("Something is wrong"), a visible focus ring, Enter and Space to press.
    /// </summary>
    public sealed class Tile : Control, IMeasured
    {
        readonly Skin skin;
        bool hover, down;
        public TileStyle Style { get; set; }
        public string Tag2 { get; set; }
        public string TagKind { get; set; }
        public string Detail { get; set; }
        public bool Selected { get; set; }
        public bool Expander { get; set; }
        public bool Expanded { get; set; }
        public int FixedWidth { get; set; }
        public Tile(Skin skin, string text, TileStyle style)
        {
            this.skin = skin; Style = style; Text = text;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.StandardClick, true);
            TabStop = true; Cursor = Cursors.Hand;
            AccessibleRole = style == TileStyle.Tab ? AccessibleRole.PageTab : style == TileStyle.Link ? AccessibleRole.Link : AccessibleRole.PushButton;
        }
        protected override void OnTextChanged(EventArgs e) { UpdateName(); base.OnTextChanged(e); Invalidate(); }
        public void UpdateName() => AccessibleName = (string.IsNullOrEmpty(Tag2) ? "" : Tag2 + ": ") + Text + (Expander ? (Expanded ? ", open" : ", closed") : "") + (Selected && Style == TileStyle.Tab ? ", selected" : "");
        Padding Inner => Style == TileStyle.Link ? new Padding(skin.T.Px(4), skin.T.Px(6), skin.T.Px(4), skin.T.Px(6)) : new Padding(skin.T.Px(10), skin.T.Px(8), skin.T.Px(10), skin.T.Px(8));
        Font MainFont => Style == TileStyle.Link ? skin.T.Body : Style == TileStyle.Row ? skin.T.Body : Style == TileStyle.HeaderToggle ? skin.T.SmallBold : skin.T.Bold;
        int TagHeight => string.IsNullOrEmpty(Tag2) ? 0 : TextRenderer.MeasureText(Tag2, skin.T.SmallBold).Height + skin.T.Px(4);
        int ChevronRoom => Expander ? skin.T.Px(22) : 0;
        string cacheKey; int cacheH;
        public int MeasureHeight(int width)
        {
            var key = width + "|" + MainFont.GetHashCode() + "|" + skin.T.Scale + "|" + Style + "|" + Tag2 + "|" + Detail + "|" + Expander + "|" + Text;
            if (key == cacheKey) return cacheH;
            cacheH = MeasureUncached(width); cacheKey = key; return cacheH;
        }
        int MeasureUncached(int width)
        {
            var t = skin.T; var p = Inner;
            int min = Style == TileStyle.Link ? t.Px(32) : Style == TileStyle.HeaderToggle ? t.Px(28) : t.Px(36);
            int textW = width - p.Horizontal - ChevronRoom;
            int h = p.Vertical + TagHeight + Text2Height(Text, MainFont, textW) + (string.IsNullOrEmpty(Detail) ? 0 : t.Px(2) + Text2Height(Detail, t.Small, textW));
            return Math.Max(min, h);
        }
        static int Text2Height(string s, Font f, int w) => global::IdeaUnstuck.AddIn.Ui.Text.Height(s, f, w);
        protected override void OnMouseEnter(EventArgs e) { hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { hover = false; down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override bool IsInputKey(Keys keyData) => keyData == Keys.Enter || keyData == Keys.Space || base.IsInputKey(keyData);
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) { e.Handled = true; e.SuppressKeyPress = true; OnClick(EventArgs.Empty); return; }
            base.OnKeyDown(e);
        }
        public void PerformClick() => OnClick(EventArgs.Empty);
        protected override void OnPaint(PaintEventArgs e)
        {
            var t = skin.T; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fill, ink, edge;
            switch (Style)
            {
                case TileStyle.Primary: fill = hover || down ? t.BlueHover : t.Blue; ink = t.BlueInk; edge = fill; break;
                case TileStyle.Success: fill = t.Green; ink = t.GreenInk; edge = fill; break;
                case TileStyle.Tab: fill = Selected ? t.Page : t.Panel; ink = t.Ink; edge = t.PanelBorder; break;
                case TileStyle.HeaderToggle: fill = Selected ? t.HeaderInk : t.Header; ink = Selected ? t.Header : t.HeaderInk; edge = t.HeaderInk; break;
                case TileStyle.Link: fill = hover ? t.RowHover : Parent?.BackColor ?? t.Page; ink = t.Link; edge = fill; break;
                case TileStyle.Group: fill = hover ? t.RowHover : t.Panel; ink = t.Ink; edge = t.PanelBorder; break;
                case TileStyle.Row: fill = hover ? t.RowHover : t.Row; ink = t.Ink; edge = t.PanelBorder; break;
                default: fill = hover ? t.RowHover : t.Row; ink = t.Ink; edge = t.PanelBorder; break;
            }
            if (t.HighContrast && hover && Style != TileStyle.Primary && Style != TileStyle.Success) { fill = SystemColors.Highlight; ink = SystemColors.HighlightText; }
            using (var b = new SolidBrush(fill)) g.FillRectangle(b, r);
            if (Style != TileStyle.Link)
                using (var pen = new Pen(edge)) g.DrawRectangle(pen, r);
            if (Style == TileStyle.Tab && Selected)
                using (var b = new SolidBrush(t.Blue)) g.FillRectangle(b, 0, Height - t.Px(3), Width, t.Px(3));
            var p = Inner; int y = p.Top; int textW = Width - p.Horizontal - ChevronRoom;
            if (!string.IsNullOrEmpty(Tag2))
            {
                var sz = TextRenderer.MeasureText(Tag2, t.SmallBold);
                var tagRect = new Rectangle(p.Left, y, sz.Width + t.Px(8), sz.Height + t.Px(1));
                var kc = t.KindColor(TagKind);
                using (var b = new SolidBrush(t.HighContrast ? fill : Color.FromArgb(28, kc))) g.FillRectangle(b, tagRect);
                TextRenderer.DrawText(g, Tag2, t.SmallBold, tagRect, t.HighContrast ? ink : kc, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                y += TagHeight;
            }
            var flags = global::IdeaUnstuck.AddIn.Ui.Text.Wrap | (Style == TileStyle.HeaderToggle || Style == TileStyle.Tab ? TextFormatFlags.HorizontalCenter : 0);
            int mainH = Text2Height(Text, MainFont, textW);
            int detailH = string.IsNullOrEmpty(Detail) ? 0 : t.Px(2) + Text2Height(Detail, t.Small, textW);
            int free = Height - p.Vertical - (y - p.Top) - mainH - detailH;
            if (string.IsNullOrEmpty(Tag2) && free > 0) y += free / 2;
            TextRenderer.DrawText(g, Text, MainFont, new Rectangle(p.Left, y, textW, mainH + 2), ink, flags);
            if (Style == TileStyle.Link && hover && !t.HighContrast)
                using (var pen = new Pen(ink)) { int w = Math.Min(textW, TextRenderer.MeasureText(Text, MainFont).Width); g.DrawLine(pen, p.Left, y + mainH, p.Left + w, y + mainH); }
            if (detailH > 0) TextRenderer.DrawText(g, Detail, t.Small, new Rectangle(p.Left, y + mainH + t.Px(2), textW, detailH), Style == TileStyle.Primary || Style == TileStyle.Success ? ink : t.InkSoft, global::IdeaUnstuck.AddIn.Ui.Text.Wrap);
            if (Expander)
            {
                int cx = Width - p.Right - t.Px(8), cy = Height / 2, s = t.Px(4);
                var pts = Expanded ? new[] { new Point(cx - s, cy - s / 2), new Point(cx + s, cy - s / 2), new Point(cx, cy + s) } : new[] { new Point(cx - s / 2, cy - s), new Point(cx - s / 2, cy + s), new Point(cx + s, cy) };
                using (var b = new SolidBrush(ink)) g.FillPolygon(b, pts);
            }
            if (Focused && ShowFocusCues)
                using (var pen = new Pen(Style == TileStyle.Primary || Style == TileStyle.Success || Style == TileStyle.HeaderToggle ? ink : t.Focus, Math.Max(2, t.Px(2))) { DashStyle = DashStyle.Dot })
                    g.DrawRectangle(pen, t.Px(2), t.Px(2), Width - t.Px(5), Height - t.Px(5));
        }
    }

    /// <summary>
    /// Vertical stack. Every child gets the full width and the height it asks for,
    /// so nothing hugs its text and nothing can be wider than the pane. With
    /// Scrolls=true it is the pane body: vertical scrolling only.
    /// </summary>
    public class Column : Panel, IMeasured
    {
        protected readonly Skin skin;
        public int Gap { get; set; } = 6;
        public bool Scrolls { get; }
        public bool Card { get; set; }
        public Color? Edge { get; set; }
        public Column(Skin skin, bool scrolls = false)
        {
            this.skin = skin; Scrolls = scrolls; AutoScroll = scrolls;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            AccessibleRole = AccessibleRole.Grouping;
        }
        static int HeightOf(Control c, int w) => c is IMeasured m ? m.MeasureHeight(w) : c is TextBox tb ? tb.PreferredHeight : c.Height;
        // Control.Visible answers false for every child until the whole window is
        // on screen, so layout reads the control's own visible flag instead.
        static readonly System.Reflection.MethodInfo getState = typeof(Control).GetMethod("GetState", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new[] { typeof(int) }, null);
        public static bool IsShown(Control c) => getState == null ? c.Visible : (bool)getState.Invoke(c, new object[] { 2 });
        int Content(int width, bool place)
        {
            var t = skin.T; int gap = t.Px(Gap); int y = Padding.Top; bool first = true;
            int w = width - Padding.Horizontal;
            foreach (Control c in Controls)
            {
                if (!IsShown(c)) continue;
                if (!first) y += gap;
                first = false;
                int h = HeightOf(c, w);
                if (place) c.SetBounds(Padding.Left, y + (Scrolls ? AutoScrollPosition.Y : 0), w, h);
                y += h;
            }
            return y + Padding.Bottom;
        }
        public int MeasureHeight(int width) => Content(width, false);
        protected override void OnLayout(LayoutEventArgs levent)
        {
            if (!Scrolls) { Content(Width, true); return; }
            int full = Width; int h = Content(full, false);
            bool bar = h > Height;
            int width = bar ? full - SystemInformation.VerticalScrollBarWidth : full;
            int total = Content(width, true);
            AutoScrollMinSize = new Size(0, bar ? total : 0);
            if (HorizontalScroll.Visible) { HorizontalScroll.Visible = false; HorizontalScroll.Enabled = false; }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Card || Edge.HasValue)
            {
                var t = skin.T;
                using (var pen = new Pen(Edge ?? t.PanelBorder)) e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
                if (Edge.HasValue && !t.HighContrast) using (var b = new SolidBrush(Edge.Value)) e.Graphics.FillRectangle(b, 0, 0, t.Px(4), Height);
            }
        }
        public void Relayout() { PerformLayout(); Invalidate(); }
    }

    /// <summary>Children side by side with equal widths (Yes / Not yet).</summary>
    public sealed class Row : Panel, IMeasured
    {
        readonly Skin skin;
        public Row(Skin skin) { this.skin = skin; TabStop = false; }
        int Place(int width, bool place)
        {
            var kids = Controls.Cast<Control>().ToList(); if (kids.Count == 0) return 0;
            int gap = skin.T.Px(6); int w = (width - gap * (kids.Count - 1)) / kids.Count; int h = 0;
            foreach (var c in kids) h = Math.Max(h, c is IMeasured m ? m.MeasureHeight(w) : c.Height);
            if (place) for (int i = 0; i < kids.Count; i++) kids[i].SetBounds(i * (w + gap), 0, i == kids.Count - 1 ? width - i * (w + gap) : w, h);
            return h;
        }
        public int MeasureHeight(int width) => Place(width, false);
        protected override void OnLayout(LayoutEventArgs e) => Place(Width, true);
    }

    /// <summary>One numbered answer step with its "where to look" tag.</summary>
    public sealed class StepView : Control, IMeasured
    {
        readonly Skin skin;
        public int Number { get; }
        public string Where { get; }
        public StepView(Skin skin, int number, string text, string where)
        {
            this.skin = skin; Number = number; Where = where; Text = text;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false); TabStop = false; BackColor = Color.Transparent;
            AccessibleRole = AccessibleRole.ListItem; AccessibleName = "Step " + number + ". " + (string.IsNullOrEmpty(where) ? "" : where + ". ") + text;
        }
        int Gutter => skin.T.Px(26);
        int TagH => string.IsNullOrEmpty(Where) ? 0 : TextRenderer.MeasureText(Where, skin.T.SmallBold).Height + skin.T.Px(4);
        string cacheKey; int cacheH;
        public int MeasureHeight(int width)
        {
            var key = width + "|" + skin.T.Body.GetHashCode() + "|" + skin.T.Scale;
            if (key == cacheKey) return cacheH;
            cacheH = Math.Max(skin.T.Px(24), TagH + global::IdeaUnstuck.AddIn.Ui.Text.Height(Text, skin.T.Body, width - Gutter) + skin.T.Px(4)); cacheKey = key; return cacheH;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var t = skin.T; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            int d = t.Px(20);
            using (var b = new SolidBrush(t.Blue)) g.FillEllipse(b, 0, t.Px(1), d, d);
            TextRenderer.DrawText(g, Number.ToString(), t.SmallBold, new Rectangle(0, t.Px(1), d, d), t.BlueInk, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            int y = 0;
            if (!string.IsNullOrEmpty(Where))
            {
                var sz = TextRenderer.MeasureText(Where, t.SmallBold);
                var r = new Rectangle(Gutter, 0, sz.Width + t.Px(8), sz.Height + t.Px(1));
                using (var b = new SolidBrush(t.Panel)) g.FillRectangle(b, r);
                using (var pen = new Pen(t.PanelBorder)) g.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
                TextRenderer.DrawText(g, Where, t.SmallBold, r, t.InkSoft, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                y = TagH;
            }
            TextRenderer.DrawText(g, Text, t.Body, new Rectangle(Gutter, y, Width - Gutter, Height - y + 2), t.Ink, global::IdeaUnstuck.AddIn.Ui.Text.Wrap);
        }
    }

    /// <summary>
    /// A real CheckBox (keyboard, screen readers and UI Automation see a check box)
    /// that draws its own box at the pane's scale, because the system box stays 13 px
    /// at 150 percent.
    /// </summary>
    public sealed class ScaledCheckBox : CheckBox
    {
        readonly Skin skin;
        public ScaledCheckBox(Skin skin)
        {
            this.skin = skin;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var t = skin.T; var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(Parent?.BackColor ?? t.Page)) g.FillRectangle(b, ClientRectangle);
            int size = Math.Min(Width, Height) - 2; var r = new Rectangle(1, 1, size - 1, size - 1);
            using (var b = new SolidBrush(Checked ? t.Green : t.Row)) g.FillRectangle(b, r);
            using (var pen = new Pen(Checked ? t.Green : t.InkSoft, Math.Max(1, t.Px(1.5f)))) g.DrawRectangle(pen, r);
            if (Checked)
                using (var pen = new Pen(t.GreenInk, Math.Max(2, t.Px(2.5f))) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                    g.DrawLines(pen, new[] { new PointF(r.X + size * .22f, r.Y + size * .52f), new PointF(r.X + size * .42f, r.Y + size * .72f), new PointF(r.X + size * .78f, r.Y + size * .3f) });
            if (Focused && ShowFocusCues) using (var pen = new Pen(t.Focus) { DashStyle = DashStyle.Dot }) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }

    /// <summary>A practice step: a real check box that SOLIDWORKS state ticks, the step text, and its links.</summary>
    public sealed class CheckRow : Panel, IMeasured
    {
        readonly Skin skin;
        public readonly ScaledCheckBox Box;
        public readonly Words Label;
        public readonly List<Control> Links = new List<Control>();
        public CheckRow(Skin skin, string text, bool done)
        {
            this.skin = skin; TabStop = false;
            Box = new ScaledCheckBox(skin) { AutoCheck = false, Checked = done, Text = "", AccessibleName = (done ? "Done: " : "Not done yet: ") + text, TabStop = true };
            Label = new Words(skin, text) { Soft = done };
            Controls.Add(Box); Controls.Add(Label);
        }
        public T AddLink<T>(T link) where T : Control { Links.Add(link); Controls.Add(link); return link; }
        int Gutter => skin.T.Px(28);
        int Place(int width, bool place)
        {
            var t = skin.T; int textW = width - Gutter;
            int lh = Label.MeasureHeight(textW); int y = Math.Max(lh, t.Px(22));
            if (place) { Box.SetBounds(0, 0, t.Px(22), t.Px(22)); Label.SetBounds(Gutter, t.Px(1), textW, lh); }
            foreach (var l in Links) { if (!Column.IsShown(l)) continue; int h = ((IMeasured)l).MeasureHeight(textW); if (place) l.SetBounds(Gutter, y, textW, h); y += h; }
            return y + t.Px(2);
        }
        public int MeasureHeight(int width) => Place(width, false);
        protected override void OnLayout(LayoutEventArgs e) => Place(Width, true);
    }

    /// <summary>A text box that draws its own gray hint while empty. Drawn after the
    /// box paints (WM_PAINT) and when it is printed (WM_PRINT, used by the harness),
    /// so it does not depend on the host process having visual styles.</summary>
    public sealed class HintTextBox : TextBox
    {
        public string Hint { get; set; } = "";
        public Color HintColor { get; set; } = Color.Gray;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            const int WM_PAINT = 0x000F, WM_PRINT = 0x0317, WM_PRINTCLIENT = 0x0318;
            // Check the message first: TextLength itself sends WM_GETTEXTLENGTH to this window.
            if (m.Msg != WM_PAINT && m.Msg != WM_PRINT && m.Msg != WM_PRINTCLIENT) return;
            if (string.IsNullOrEmpty(Hint) || TextLength != 0) return;
            if (m.Msg == WM_PAINT) using (var g = CreateGraphics()) DrawHint(g);
            else if ((m.Msg == WM_PRINT || m.Msg == WM_PRINTCLIENT) && m.WParam != IntPtr.Zero) using (var g = Graphics.FromHdc(m.WParam)) DrawHint(g);
        }
        void DrawHint(Graphics g) => TextRenderer.DrawText(g, Hint, Font, new Rectangle(1, 0, ClientSize.Width - 1, ClientSize.Height), HintColor, BackColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    }

    /// <summary>The search box: a bordered white strip holding the text box.</summary>
    public sealed class SearchBox : Panel, IMeasured
    {
        readonly Skin skin;
        public readonly HintTextBox Box;
        public SearchBox(Skin skin, string placeholder)
        {
            this.skin = skin; TabStop = false;
            Box = new HintTextBox { Name = "HelpSearch", AccessibleName = "Search. " + placeholder, Hint = placeholder, MaxLength = 200, BorderStyle = BorderStyle.None, TabIndex = 0 };
            Controls.Add(Box);
            BackColor = Color.White;
            // A click on the strip around the text is a click on the box.
            Click += (s, e) => Box.Focus();
            Cursor = Cursors.IBeam;
        }
        public void ApplyTheme() { Box.Font = skin.T.Body; Box.BackColor = skin.T.Row; Box.ForeColor = skin.T.Ink; Box.HintColor = skin.T.InkSoft; BackColor = skin.T.Row; }
        public int MeasureHeight(int width) => Math.Max(skin.T.Px(38), Box.PreferredHeight + skin.T.Px(16));
        protected override void OnLayout(LayoutEventArgs e)
        {
            var t = skin.T; int pad = t.Px(9); int boxH = Box.PreferredHeight;
            Box.SetBounds(pad, (Height - boxH) / 2, Width - pad * 2, boxH);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var t = skin.T; bool focused = Box.Focused;
            int w = focused ? Math.Max(2, t.Px(2)) : 1;
            using (var pen = new Pen(focused ? t.Blue : t.PanelBorder, w) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset })
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }
    }
}
