using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace A4TechControl;

public static class Theme
{
    public static readonly Color Window = Color.FromArgb(255, 24, 27, 33);
    public static readonly Color Card = Color.FromArgb(255, 33, 37, 45);
    public static readonly Color CardHover = Color.FromArgb(255, 46, 51, 62);
    public static readonly Color Border = Color.FromArgb(255, 58, 64, 75);
    public static readonly Color Text = Color.FromArgb(255, 231, 235, 243);
    public static readonly Color Muted = Color.FromArgb(255, 148, 158, 176);
    public static readonly Color Accent = Color.FromArgb(255, 255, 159, 28);
    public static readonly Color AccentDark = Color.FromArgb(255, 226, 132, 12);
    public static readonly Color Input = Color.FromArgb(255, 46, 51, 62);
}

public static class ThemeDraw
{
    public static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        int d = Math.Min(radius, Math.Min(r.Width, r.Height) / 2);
        var path = new GraphicsPath();
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void DrawCheckMark(Graphics g, Point p, Color color)
    {
        using var pen = new Pen(color, 2);
        g.DrawLine(pen, p.X + 3, p.Y + 8, p.X + 6, p.Y + 11);
        g.DrawLine(pen, p.X + 6, p.Y + 11, p.X + 12, p.Y + 4);
    }
}

/// <summary>Yuvarlatılmış kart paneli.</summary>
public sealed class ModernPanel : Panel
{
    public ModernPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Card;
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnPaint(PaintEventArgs e)
    {
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = ThemeDraw.RoundedRect(rect, 12);
        using var brush = new SolidBrush(Theme.Card);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawPath(pen, path);
    }
}

/// <summary>Düz, yuvarlatılmış, vurgulu buton.</summary>
public sealed class ModernButton : Button
{
    private bool _hover;

    public bool IsAccent { get; set; }

    public ModernButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        FlatStyle = FlatStyle.Flat;
        BackColor = Theme.Card;
        ForeColor = Theme.Text;
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = ThemeDraw.RoundedRect(rect, 8);
        var color = !Enabled
            ? Theme.Border
            : IsAccent
                ? (_hover ? Theme.Accent : Theme.AccentDark)
                : (_hover ? Theme.CardHover : Theme.Card);

        using var brush = new SolidBrush(color);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
        using var pen = new Pen(IsAccent ? Theme.Accent : Theme.Border);
        e.Graphics.DrawPath(pen, path);

        TextRenderer.DrawText(e.Graphics, Text, Font, rect,
            IsAccent ? Theme.Window : Theme.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

/// <summary>Turuncu kare + tik çizen modern onay kutusu.</summary>
public sealed class ModernCheckBox : CheckBox
{
    public ModernCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        AutoSize = false;               // CheckBox varsayılanı uzun metinde kutuyu büyütüyor
        BackColor = Theme.Card;
        ForeColor = Theme.Text;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);
        const int box = 16;
        int y = (Height - box) / 2;
        using var path = ThemeDraw.RoundedRect(new Rectangle(0, y, box, box), 4);
        using var brush = new SolidBrush(Checked ? Theme.Accent : Theme.Input);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
        using var pen = new Pen(Checked ? Theme.Accent : Theme.Border);
        e.Graphics.DrawPath(pen, path);
        if (Checked)
            ThemeDraw.DrawCheckMark(e.Graphics, new Point(0, y), Theme.Window);

        TextRenderer.DrawText(e.Graphics, Text, Font,
            new Rectangle(box + 9, 0, Width - box - 9, Height),
            Theme.Text, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}

/// <summary>Tepsi menüsü için koyu, düz görünüm.</summary>
public sealed class ModernMenuRenderer : ToolStripProfessionalRenderer
{
    public ModernMenuRenderer() : base(new ModernColorTable()) { }

    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
    {
        e.Graphics.Clear(Theme.Card);
    }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item is ToolStripSeparator) return;
        var rect = new Rectangle(2, 1, e.Item.Size.Width - 4, e.Item.Size.Height - 2);
        using var path = ThemeDraw.RoundedRect(rect, 5);
        using var brush = new SolidBrush(e.Item.Selected || e.Item.Pressed ? Theme.CardHover : Theme.Card);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        e.Graphics.Clear(Theme.Card);
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawLine(pen, 6, e.Item.Size.Height / 2, e.Item.Size.Width - 6, e.Item.Size.Height / 2);
    }

    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        var color = !e.Item.Enabled
            ? Theme.Muted
            : (e.Item.Selected || e.Item.Pressed ? Theme.Accent : Theme.Text);
        TextRenderer.DrawText(e.Graphics, e.Text, e.ToolStrip?.Font ?? SystemFonts.MenuFont,
            e.TextRectangle, color, e.TextFormat);
    }

    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        using var path = ThemeDraw.RoundedRect(new Rectangle(e.ImageRectangle.X, e.ImageRectangle.Y, 14, 14), 3);
        using var brush = new SolidBrush(Theme.Accent);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
        ThemeDraw.DrawCheckMark(e.Graphics, new Point(e.ImageRectangle.X, e.ImageRectangle.Y), Theme.Window);
    }

    private sealed class ModernColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder => Theme.Border;
        public override Color ToolStripDropDownBackground => Theme.Card;
        public override Color MenuItemBorder => Theme.CardHover;
        public override Color MenuItemSelected => Theme.CardHover;
        public override Color MenuItemSelectedGradientBegin => Theme.CardHover;
        public override Color MenuItemSelectedGradientEnd => Theme.CardHover;
        public override Color MenuStripGradientBegin => Theme.Card;
        public override Color MenuStripGradientEnd => Theme.Card;
        public override Color CheckBackground => Theme.Accent;
        public override Color ButtonSelectedHighlight => Theme.CardHover;
        public override Color ButtonPressedHighlight => Theme.CardHover;
    }
}
