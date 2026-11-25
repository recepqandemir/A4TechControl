namespace A4TechControl;

/// <summary>
/// FG35'in alıcı hafızasında kalmış olabilecek ses/medya tuşu yankılarını engeller.
/// Donanım yankısı ile bizim SendInput enjeksiyonumuz üst üste binince ses iki kademe değişiyordu.
/// </summary>
public static class VolumeEchoGuard
{
    private static readonly Dictionary<ushort, long> Sent = new();
    private static readonly object Sync = new();

    /// <summary>Yan tuşlardan biri basılıyken donanımdan gelen medya tuşlarını sustur.</summary>
    public static bool SideButtonHeld { get; set; }

    public static bool IsMediaKey(ushort vk) => vk is >= 0xA6 and <= 0xB7;

    public static void Mark(ushort vk)
    {
        lock (Sync) Sent[vk] = Environment.TickCount64;
    }

    public static bool ShouldSuppress(ushort vk)
    {
        if (!IsMediaKey(vk)) return false;
        if (SideButtonHeld) return true;

        lock (Sync)
        {
            return Sent.TryGetValue(vk, out var time) && Environment.TickCount64 - time <= 200;
        }
    }
}
