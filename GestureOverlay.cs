using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using static A4TechControl.NativeMethods;

namespace A4TechControl;

/// <summary>
/// Jest çizilirken imlecin yanında görünen, tıklamaları geçiren şeffaf ok katmanı.
/// Tam ekran yerine küçük bir pencere kullanılır; çoklu monitörlerde kırpılma olmaz.
/// </summary>
public sealed class GestureOverlay : Form, IGestureFeedback
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int w, int h, uint flags);

    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpNoActivate = 0x0010;

    private POINT _start;
    private POINT _current;
    private string? _direction;
    private string? _action;
    private Point _origin;
    private bool _active;

    public GestureOverlay()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        StartPosition = FormStartPosition.Manual;
        ClientSize = new Size(520, 300);
        DoubleBuffered = true;
        Text = "GestureOverlay";
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x00000020;  // WS_EX_TRANSPARENT -> fare olayları alt pencereye geçer
            cp.ExStyle |= 0x00000080;  // WS_EX_TOOLWINDOW -> Alt+Tab'da görünmez
            return cp;
        }
    }

    public void Update(POINT start, POINT current, string? direction, string? action)
    {
        if (!_active)
        {
            _active = true;
            Show();
        }

        int x = Math.Min(start.X, current.X) - 40;
        int y = Math.Min(start.Y, current.Y) - 40;
        int w = Math.Clamp(Math.Abs(start.X - current.X) + 200, 260, 900);
        int h = Math.Clamp(Math.Abs(start.Y - current.Y) + 140, 180, 600);

        _origin = new Point(x, y);
        SetWindowPos(Handle, HwndTopmost, x, y, w, h, SwpNoActivate);

        _start = start;
        _current = current;
        _direction = direction;
        _action = action;
        Invalidate();
    }

    public new void Hide()
    {
        if (!_active) return;
        _active = false;
        base.Hide();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_active) return;
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var start = new Point(_start.X - _origin.X, _start.Y - _origin.Y);
        var end = new Point(_current.X - _origin.X, _current.Y - _origin.Y);

        using var glow = new Pen(Color.FromArgb(90, 255, 170, 45), 12);
        using var line = new Pen(Color.FromArgb(255, 255, 190, 60), 5);
        g.DrawLine(glow, start, end);
        g.DrawLine(line, start, end);

        double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
        const double spread = Math.PI / 7;
        const int headLength = 26;
        var wing1 = new PointF(
            end.X - headLength * (float)Math.Cos(angle - spread),
            end.Y - headLength * (float)Math.Sin(angle - spread));
        var wing2 = new PointF(
            end.X - headLength * (float)Math.Cos(angle + spread),
            end.Y - headLength * (float)Math.Sin(angle + spread));
        g.DrawLine(line, end, wing1);
        g.DrawLine(line, end, wing2);

        using var startBrush = new SolidBrush(Color.FromArgb(255, 255, 165, 55));
        g.FillEllipse(startBrush, start.X - 8, start.Y - 8, 16, 16);
        using var startBorder = new Pen(Color.White, 2);
        g.DrawEllipse(startBorder, start.X - 8, start.Y - 8, 16, 16);

        var directionText = _direction is null ? "Jest" : GestureNames.Display(_direction);
        var actionText = string.IsNullOrWhiteSpace(_action) ? string.Empty : $"   →   {_action}";
        var label = directionText + actionText;

        using var font = new Font("Segoe UI", 11F, FontStyle.Bold);
        var size = g.MeasureString(label, font);
        int boxW = (int)size.Width + 24;
        int boxH = (int)size.Height + 12;
        int lx = end.X + 20;
        int ly = end.Y - boxH / 2;
        if (lx + boxW > Width - 6) lx = end.X - boxW - 20;
        if (ly < 6) ly = 6;
        if (ly + boxH > Height - 6) ly = Height - boxH - 6;

        using var boxBrush = new SolidBrush(Color.FromArgb(230, 30, 33, 40));
        using var boxBorder = new Pen(Color.FromArgb(255, 255, 165, 55), 2);
        g.FillRectangle(boxBrush, lx, ly, boxW, boxH);
        g.DrawRectangle(boxBorder, lx, ly, boxW, boxH);
        g.DrawString(label, font, Brushes.White, lx + 12, ly + 6);
    }
}
