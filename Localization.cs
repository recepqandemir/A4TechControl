namespace A4TechControl;

/// <summary>TR / EN arayüz metinleri. Dil ayarı AppSettings içinde saklanır.</summary>
public static class Lang
{
    public const string Turkish = "TR";
    public const string English = "EN";

    public static string Current { get; private set; } = Turkish;
    public static event Action? Changed;

    private sealed record Entry(string Tr, string En);

    private static readonly Dictionary<string, Entry> Strings = new(StringComparer.OrdinalIgnoreCase)
    {
        // Tepsi menüsü
        ["menu.settings"] = new("Ayarlar...", "Settings..."),
        ["menu.swap"] = new("4. ve 5. tuş takası", "Swap buttons 4 and 5"),
        ["menu.gestures"] = new("Jestler", "Gestures"),
        ["menu.gestureArrow"] = new("Jest okunu göster", "Show gesture arrow"),
        ["menu.fn"] = new("Fn tuş eşlemeleri", "Fn key mappings"),
        ["menu.wheel"] = new("4 yönlü tekerlek", "4-way wheel"),
        ["menu.autostart"] = new("Açılışta başlat", "Launch at startup"),
        ["menu.version"] = new("Sürüm {0}", "Version {0}"),
        ["menu.exit"] = new("Çıkış", "Exit"),
        ["tray.text"] = new("A4Tech Control (FG35 / FGK10) — Ayarlar için çift tıkla",
                            "A4Tech Control (FG35 / FGK10) — Double-click for settings"),
        ["balloon.title"] = new("A4Tech Control", "A4Tech Control"),
        ["balloon.text"] = new("FG35 / FGK10 hazır. Ayarlar için tepsi ikonuna çift tıkla.",
                               "FG35 / FGK10 ready. Double-click the tray icon for settings."),

        // Ayarlar başlıkları
        ["card.mouse"] = new("FARE TUŞLARI", "MOUSE BUTTONS"),
        ["card.gestures"] = new("JESTLER   (k: 55-130 px, u: ≥130 px, en az 200 ms)",
                                "GESTURES   (s: 55-130 px, l: ≥130 px, min 200 ms)"),
        ["card.general"] = new("GENEL", "GENERAL"),
        ["card.fn"] = new("KLAVYE — Fn TUŞLARI   (Öğren: tuşa basarak kodu yakala)",
                          "KEYBOARD — Fn KEYS   (Learn: press a key to capture it)"),

        // Etiketler ve onay kutuları
        ["lbl.button3"] = new("Tuş 3 (tekerlek):", "Button 3 (wheel click):"),
        ["lbl.button4"] = new("Tuş 4 (yan tuş 1):", "Button 4 (side button 1):"),
        ["lbl.button5"] = new("Tuş 5 (yan tuş 2):", "Button 5 (side button 2):"),
        ["chk.swap"] = new("4. ve 5. tuşları takas et", "Swap buttons 4 and 5"),
        ["chk.gestures"] = new("Jestleri etkinleştir (sağ tuş basılı tut + hareket)",
                               "Enable gestures (hold right button + move)"),
        ["chk.wheel"] = new("4 yönlü tekerleği etkinleştir", "Enable 4-way wheel"),
        ["chk.autostart"] = new("Windows ile birlikte başlat", "Launch with Windows"),
        ["chk.overlay"] = new("Jest sırasında ekranda ok göster", "Show arrow on screen during gesture"),
        ["chk.fn"] = new("Fn tuş eşlemelerini etkinleştir", "Enable Fn key mappings"),
        ["btn.save"] = new("Kaydet", "Save"),
        ["btn.cancel"] = new("Vazgeç", "Cancel"),
        ["btn.learn"] = new("Öğren", "Learn"),
        ["version.label"] = new("A4Tech Control  •  Sürüm {0}", "A4Tech Control  •  Version {0}"),

        // Durum mesajları
        ["status.capturing"] = new("{0} için bir tuşa basın... (Escape = iptal)",
                                   "Press a key for {0}... (Escape = cancel)"),
        ["status.cancelled"] = new("Öğrenme iptal edildi.", "Learning cancelled."),
        ["status.learned"] = new("{0} → {1} (0x{2:X2}) öğrenildi. Kaydet'e basmayı unutmayın.",
                                 "{0} → {1} (0x{2:X2}) learned. Don't forget to click Save."),
        ["validation.title"] = new("Geçersiz ayar", "Invalid setting"),
        ["validation.message"] = new("\"{0}\" için geçersiz eylem: \"{1}\"\n\nÖrnek: VolumeUp, Mute, Ctrl+Shift+A",
                                     "Invalid action for \"{0}\": \"{1}\"\n\nExample: VolumeUp, Mute, Ctrl+Shift+A"),

        // Jest yönleri
        ["gesture.Right"] = new("Sağ →", "Right →"),
        ["gesture.UpRight"] = new("Sağ-Yuk. ↗", "Up-Right ↗"),
        ["gesture.Up"] = new("Yukarı ↑", "Up ↑"),
        ["gesture.UpLeft"] = new("Sol-Yuk. ↖", "Up-Left ↖"),
        ["gesture.Left"] = new("Sol ←", "Left ←"),
        ["gesture.DownLeft"] = new("Sol-Aş. ↙", "Down-Left ↙"),
        ["gesture.Down"] = new("Aşağı ↓", "Down ↓"),
        ["gesture.DownRight"] = new("Sağ-Aş. ↘", "Down-Right ↘"),
        ["gesture.short"] = new(" (k)", " (s)"),
        ["gesture.long"] = new(" (u)", " (l)"),
        ["gesture.hint"] = new("Jest modu — tuşu bırakınca eylem uygulanır",
                               "Gesture mode — action runs when you release the button")
    };

    public static string T(string key) =>
        Strings.TryGetValue(key, out var entry) ? (Current == English ? entry.En : entry.Tr) : key;

    public static string F(string key, params object[] args) => string.Format(T(key), args);

    public static void Set(string code, bool persist = true)
    {
        var normalized = code.Equals(English, StringComparison.OrdinalIgnoreCase) ? English : Turkish;
        if (Current == normalized) return;
        Current = normalized;
        if (persist)
        {
            AppSettings.Current.Language = normalized;
            AppSettings.Current.Save();
        }
        Changed?.Invoke();
    }
}
