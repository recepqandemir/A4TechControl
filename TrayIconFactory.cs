using System.Drawing.Drawing2D;

namespace A4TechControl;

/// <summary>Tanınabilir, özel tepsi ikonu (mavi "i" yerine turuncu "A").</summary>
public static class TrayIconFactory
{
    private static Icon? _icon;

    public static Icon Create()
    {
        if (_icon is not null) return _icon;

        // Önce projeye gömülü app.ico dosyasını kullan (exe ikonuyla birebir aynı).
        try
        {
            using var stream = typeof(TrayIconFactory).Assembly
                .GetManifestResourceStream("A4TechControl.Resources.app.ico");
            if (stream is not null)
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                _icon = new Icon(new MemoryStream(copy.ToArray()));
                return _icon;
            }
        }
        catch
        {
            // Gömülü ikon okunamazsa aşağıdaki çizilmiş ikonu kullan.
        }

        using var bitmap = new Bitmap(64, 64);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            using var body = new SolidBrush(Color.FromArgb(255, 36, 40, 48));
            using var border = new Pen(Color.FromArgb(255, 255, 165, 55), 3);
            g.FillPath(body, RoundedRect(3, 3, 58, 58, 14));
            g.DrawPath(border, RoundedRect(3, 3, 58, 58, 14));

            using var font = new Font("Segoe UI", 26F, FontStyle.Bold);
            using var textBrush = new SolidBrush(Color.FromArgb(255, 255, 175, 60));
            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString("A4", font, textBrush, new RectangleF(3, 4, 58, 54), format);

            // Fare tekerleği çizgisi
            using var wheel = new Pen(Color.FromArgb(255, 255, 210, 140), 3);
            g.DrawLine(wheel, 28, 12, 36, 12);
        }

        _icon = Icon.FromHandle(bitmap.GetHicon());
        return _icon;
    }

    private static GraphicsPath RoundedRect(int x, int y, int w, int h, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(x, y, radius, radius, 180, 90);
        path.AddArc(x + w - radius, y, radius, radius, 270, 90);
        path.AddArc(x + w - radius, y + h - radius, radius, radius, 0, 90);
        path.AddArc(x, y + h - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}
