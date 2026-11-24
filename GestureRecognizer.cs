using static A4TechCompanion.NativeMethods;

namespace A4TechCompanion;

/// <summary>Tuş basılı çizilen yönü ve mesafeyi 16 jestten birine eşler.</summary>
public static class GestureRecognizer
{
    public const int MinimumDistance = 55;

    public static string? GetDirection(POINT start, POINT end)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance < MinimumDistance) return null;

        // Ekran koordinatlarında Y aşağı doğru arttığı için -dy kullanıyoruz.
        double angle = Math.Atan2(-dy, dx) * 180.0 / Math.PI;
        if (angle < 0) angle += 360;

        int sector = (int)Math.Round(angle / 45.0) % 8;
        var baseDirection = GestureNames.BaseDirections[sector];
        return distance >= GestureNames.LongDistance
            ? baseDirection + GestureNames.LongSuffix
            : baseDirection;
    }
}
