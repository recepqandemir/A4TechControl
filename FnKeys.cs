using static A4TechCompanion.NativeMethods;

namespace A4TechCompanion;

/// <summary>Klavye Fn+F1..F12 yuvalarının dahili anahtarları ve varsayılan tetikleyicileri.</summary>
public static class FnKeys
{
    public static readonly string[] All = Enumerable.Range(1, 12).Select(i => "Fn" + i).ToArray();

    public static string Display(string key) => key switch
    {
        "Fn1" => "Fn + F1",
        "Fn2" => "Fn + F2",
        "Fn3" => "Fn + F3",
        "Fn4" => "Fn + F4",
        "Fn5" => "Fn + F5",
        "Fn6" => "Fn + F6",
        "Fn7" => "Fn + F7",
        "Fn8" => "Fn + F8",
        "Fn9" => "Fn + F9",
        "Fn10" => "Fn + F10",
        "Fn11" => "Fn + F11",
        "Fn12" => "Fn + F12",
        _ => key
    };

    /// <summary>
    /// A4Tech ofis klavyelerinin yaygın Fn eşlemeleri. Bunlar sadece başlangıç değeridir;
    /// kullanıcı "Öğren" ile kendi klavyesinin gönderdiği kodu kaydedebilir.
    /// </summary>
    public static Dictionary<string, int> DefaultTriggers() => new()
    {
        ["Fn1"] = VK_VOLUME_MUTE,
        ["Fn2"] = VK_VOLUME_DOWN,
        ["Fn3"] = VK_VOLUME_UP,
        ["Fn4"] = VK_MEDIA_PLAY_PAUSE,
        ["Fn5"] = VK_MEDIA_STOP,
        ["Fn6"] = VK_MEDIA_PREV_TRACK,
        ["Fn7"] = VK_MEDIA_NEXT_TRACK,
        ["Fn8"] = VK_BROWSER_BACK,
        ["Fn9"] = VK_BROWSER_FORWARD,
        ["Fn10"] = VK_LAUNCH_MAIL,
        ["Fn11"] = VK_LAUNCH_MEDIA_SELECT,
        ["Fn12"] = VK_LAUNCH_APP2
    };

    public static Dictionary<string, string> DefaultActions() =>
        All.ToDictionary(key => key, _ => "None");

    public static string KeyName(int vk)
    {
        if (vk < 0) return "İptal";
        if (vk == 0) return "Atanmadı";
        var name = ((System.Windows.Forms.Keys)vk).ToString();
        return name.Length > 0 && char.IsLetter(name[0]) && name.All(char.IsLetterOrDigit)
            ? name
            : $"0x{vk:X2}";
    }
}
