using System.Drawing.Drawing2D;

namespace A4TechControl;

/// <summary>
/// Koyu temalı, kendi çizdiğimiz combo. Standart ComboBox'ın açık renkli ok alanı
/// ve sabit yüksekliği satırların üst üste binmesine yol açıyordu.
/// </summary>
public sealed class ModernComboBox : Panel
{
    private readonly TextBox _text = new();
    private readonly List<string> _items = new();
    private readonly ContextMenuStrip _menu = new()
    {
        Renderer = new ModernMenuRenderer(),
        Font = new Font("Segoe UI", 9F)
    };
    private bool _hover;

    public ModernComboBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw, true);
        BackColor = Theme.Input;
        Height = 20;

        _text.BorderStyle = BorderStyle.None;
        _text.BackColor = Theme.Input;
        _text.ForeColor = Theme.Text;
        _text.Font = Font;

        Controls.Add(_text);
        _menu.ItemClicked += (_, e) => Text = e.ClickedItem.Text;

        MouseEnter += (_, _) => { _hover = true; Invalidate(); };
        MouseLeave += (_, _) => { _hover = false; Invalidate(); };
        MouseDown += OnMouseDown;
    }

    public List<string> Items => _items;

    public override string Text
    {
        get => _text.Text;
        set => _text.Text = value;
    }

    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            _text.Font = value;
        }
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        _text.SetBounds(7, 1, Math.Max(8, Width - 34), Math.Max(8, Height - 2));
        base.OnLayout(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = ThemeDraw.RoundedRect(rect, 4);
        using var brush = new SolidBrush(_hover ? Theme.CardHover : Theme.Input);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawPath(pen, path);

        int arrowX = Width - 25;
        e.Graphics.DrawLine(pen, arrowX, 3, arrowX, Height - 3);
        using var arrowBrush = new SolidBrush(Theme.Accent);
        int cy = Height / 2;
        PointF[] triangle =
        {
            new(arrowX + 8, cy - 3),
            new(arrowX + 16, cy - 3),
            new(arrowX + 12, cy + 3)
        };
        e.Graphics.FillPolygon(arrowBrush, triangle);
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.X < Width - 25)
        {
            _text.Focus();
            return;
        }

        _menu.Items.Clear();
        foreach (var item in _items)
            _menu.Items.Add(new ToolStripMenuItem(item));

        _menu.Show(this, new Point(0, Height));
    }
}
