namespace A4TechControl;

/// <summary>
/// 16 jest yönü: 8 ana yön × 2 mesafe (kısa / uzun).
/// JSON içinde daima İngilizce anahtarlar kullanılır; böylece kültür/encoding sorunu olmaz.
/// </summary>
public static class GestureNames
{
    public const string Right = "Right";
    public const string UpRight = "UpRight";
    public const string Up = "Up";
    public const string UpLeft = "UpLeft";
    public const string Left = "Left";
    public const string DownLeft = "DownLeft";
    public const string Down = "Down";
    public const string DownRight = "DownRight";

    public const string LongSuffix = "Long";
    public const int LongDistance = 130;

    /// <summary>Sıra, açı sektörleriyle birebir eşleşmelidir (0 = sağ, 1 = sağ-yukarı, ...).</summary>
    public static readonly string[] BaseDirections =
    {
        Right, UpRight, Up, UpLeft, Left, DownLeft, Down, DownRight
    };

    public static readonly string[] All = BuildAll();

    private static string[] BuildAll()
    {
        var result = new string[BaseDirections.Length * 2];
        for (int i = 0; i < BaseDirections.Length; i++)
        {
            result[i * 2] = BaseDirections[i];
            result[i * 2 + 1] = BaseDirections[i] + LongSuffix;
        }
        return result;
    }

    public static bool IsLong(string key) => key.EndsWith(LongSuffix, StringComparison.Ordinal);

    public static string Base(string key) => IsLong(key) ? key[..^LongSuffix.Length] : key;

    public static string Display(string key)
    {
        var text = Base(key) switch
        {
            Right => "Sağ →",
            UpRight => "Sağ-Yuk. ↗",
            Up => "Yukarı ↑",
            UpLeft => "Sol-Yuk. ↖",
            Left => "Sol ←",
            DownLeft => "Sol-Aş. ↙",
            Down => "Aşağı ↓",
            DownRight => "Sağ-Aş. ↘",
            _ => Base(key)
        };
        return text + (IsLong(key) ? " (u)" : " (k)");
    }

    public static Dictionary<string, string> Defaults()
    {
        var result = new Dictionary<string, string>
        {
            [Right] = "Next",
            [Right + LongSuffix] = "Forward",
            [UpRight] = "None",
            [UpRight + LongSuffix] = "None",
            [Up] = "VolumeUp",
            [Up + LongSuffix] = "Mute",
            [UpLeft] = "None",
            [UpLeft + LongSuffix] = "None",
            [Left] = "Previous",
            [Left + LongSuffix] = "Back",
            [DownLeft] = "None",
            [DownLeft + LongSuffix] = "None",
            [Down] = "VolumeDown",
            [Down + LongSuffix] = "Mute",
            [DownRight] = "None",
            [DownRight + LongSuffix] = "None"
        };
        return result;
    }
}
