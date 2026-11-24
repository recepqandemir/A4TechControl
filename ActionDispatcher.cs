namespace A4TechCompanion;

/// <summary>Bir tuşa/jeste atanan eylemi çalıştırır.</summary>
public static class ActionDispatcher
{
    /// <summary>Ayarlar penceresindeki hazır seçenekler.</summary>
    public static readonly string[] Choices =
    {
        "None",
        "VolumeUp",
        "VolumeDown",
        "Mute",
        "PlayPause",
        "Next",
        "Previous",
        "Back",
        "Forward",
        "RightClick",
        "MiddleClick"
    };

    public static bool IsValid(string? action)
    {
        if (string.IsNullOrWhiteSpace(action)) return false;
        if (Choices.Contains(action.Trim(), StringComparer.OrdinalIgnoreCase)) return true;
        return HotkeyParser.TryParse(action, out _, out _);
    }

    public static void Execute(string? action)
    {
        if (string.IsNullOrWhiteSpace(action)) return;
        var value = action.Trim();

        if (value.Equals("None", StringComparison.OrdinalIgnoreCase)) return;

        switch (value.ToLowerInvariant())
        {
            case "volumeup":
                InputSender.TapKey(NativeMethods.VK_VOLUME_UP);
                return;
            case "volumedown":
                InputSender.TapKey(NativeMethods.VK_VOLUME_DOWN);
                return;
            case "mute":
                InputSender.TapKey(NativeMethods.VK_VOLUME_MUTE);
                return;
            case "playpause":
                InputSender.TapKey(NativeMethods.VK_MEDIA_PLAY_PAUSE);
                return;
            case "next":
                InputSender.TapKey(NativeMethods.VK_MEDIA_NEXT_TRACK);
                return;
            case "previous":
                InputSender.TapKey(NativeMethods.VK_MEDIA_PREV_TRACK);
                return;
            case "back":
                InputSender.TapKey(NativeMethods.VK_BROWSER_BACK);
                return;
            case "forward":
                InputSender.TapKey(NativeMethods.VK_BROWSER_FORWARD);
                return;
            case "rightclick":
                InputSender.TapMouseRight();
                return;
        }

        if (HotkeyParser.TryParse(value, out var modifiers, out var key))
            InputSender.SendHotkey(modifiers, key);
    }
}
