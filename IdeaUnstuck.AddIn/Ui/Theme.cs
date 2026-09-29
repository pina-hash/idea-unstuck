using System;
using System.Drawing;
using System.Windows.Forms;

namespace IdeaUnstuck.AddIn.Ui
{
    /// <summary>
    /// Colors and sizes. The look follows the SolidWorks Day post: dark title bar
    /// #2c3036, light gray panels, selection blue #1f6fbf, success green #1e7a3a,
    /// Segoe UI, no orange. In Windows high contrast every color comes from
    /// SystemColors instead.
    /// </summary>
    public sealed class Theme
    {
        public static readonly float[] TextSizes = { 10f, 12f, 14f };
        public static readonly string[] TextSizeNames = { "A", "A+", "A++" };

        public bool HighContrast { get; }
        /// <summary>Pixels per 96-dpi pixel: DPI scale times the harness override.</summary>
        public float Scale { get; }
        public int TextSize { get; }

        public Color Page, Panel, PanelBorder, Ink, InkSoft, Header, HeaderInk, Blue, BlueInk, BlueHover, Green, GreenInk, Link, Row, RowHover, Focus, CardEdge, Message;
        public Font Body, Bold, Small, SmallBold, Title, HeaderFont, HeaderSmall;

        public Theme(float scale, int textSize, bool? highContrast = null, float fontScale = 1f)
        {
            Scale = scale <= 0 ? 1 : scale;
            TextSize = Math.Max(0, Math.Min(2, textSize));
            HighContrast = highContrast ?? SystemInformation.HighContrast;
            float pt = TextSizes[TextSize] * fontScale;
            Body = new Font("Segoe UI", pt, FontStyle.Regular);
            Bold = new Font("Segoe UI", pt, FontStyle.Bold);
            Small = new Font("Segoe UI", Math.Max(8.5f, pt - 1.5f) * (fontScale >= 1 ? 1 : fontScale), FontStyle.Regular);
            SmallBold = new Font("Segoe UI", Math.Max(8.5f, pt - 1.5f) * (fontScale >= 1 ? 1 : fontScale), FontStyle.Bold);
            Title = new Font("Segoe UI", pt + 2, FontStyle.Bold);
            HeaderFont = new Font("Segoe UI", 11f * fontScale, FontStyle.Bold);
            HeaderSmall = new Font("Segoe UI", 9f * fontScale, FontStyle.Bold);
            if (HighContrast)
            {
                Page = Panel = Row = SystemColors.Window; Ink = InkSoft = SystemColors.WindowText;
                PanelBorder = CardEdge = SystemColors.WindowText; Header = SystemColors.Window; HeaderInk = SystemColors.WindowText;
                Blue = SystemColors.Highlight; BlueInk = SystemColors.HighlightText; BlueHover = SystemColors.Highlight;
                Green = SystemColors.Highlight; GreenInk = SystemColors.HighlightText; Link = SystemColors.HotTrack;
                RowHover = SystemColors.Highlight; Focus = SystemColors.WindowText; Message = SystemColors.Window;
            }
            else
            {
                Page = Color.FromArgb(0xf3, 0xf3, 0xf4);
                Panel = Color.FromArgb(0xe8, 0xe9, 0xeb);
                PanelBorder = Color.FromArgb(0xc3, 0xc6, 0xcb);
                Row = Color.White;
                RowHover = Color.FromArgb(0xe3, 0xee, 0xfa);
                Ink = Color.FromArgb(0x1d, 0x21, 0x26);
                InkSoft = Color.FromArgb(0x4a, 0x50, 0x58);
                Header = Color.FromArgb(0x2c, 0x30, 0x36);
                HeaderInk = Color.White;
                Blue = Color.FromArgb(0x1f, 0x6f, 0xbf);
                BlueHover = Color.FromArgb(0x18, 0x5c, 0xa0);
                BlueInk = Color.White;
                Green = Color.FromArgb(0x1e, 0x7a, 0x3a);
                GreenInk = Color.White;
                Link = Color.FromArgb(0x1f, 0x6f, 0xbf);
                Focus = Color.FromArgb(0x1d, 0x21, 0x26);
                CardEdge = Color.FromArgb(0x1f, 0x6f, 0xbf);
                Message = Color.FromArgb(0xe3, 0xee, 0xfa);
            }
        }
        public int Px(float px) => (int)Math.Round(px * Scale);
        public Color KindColor(string kind)
        {
            if (HighContrast) return SystemColors.WindowText;
            return kind == "wrong" ? Color.FromArgb(0xa3, 0x2a, 0x2a) : kind == "how" ? Color.FromArgb(0x1f, 0x6f, 0xbf) : Color.FromArgb(0x55, 0x4f, 0x8f);
        }
    }
}
