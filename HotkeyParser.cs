using System.Windows.Forms;

namespace A4TechCompanion;

/// <summary>"Ctrl+Shift+A", "Alt+F4", "Win+E" gibi metinleri sanal tuşlara çevirir.</summary>
public static class HotkeyParser
{
    private static readonly Dictionary<string, ushort> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["esc"] = (ushort)Keys.Escape,
        ["escape"] = (ushort)Keys.Escape,
        ["space"] = (ushort)Keys.Space,
        ["enter"] = (ushort)Keys.Enter,
        ["return"] = (ushort)Keys.Enter,
        ["tab"] = (ushort)Keys.Tab,
        ["del"] = (ushort)Keys.Delete,
        ["delete"] = (ushort)Keys.Delete,
        ["ins"] = (ushort)Keys.Insert,
        ["insert"] = (ushort)Keys.Insert,
        ["home"] = (ushort)Keys.Home,
        ["end"] = (ushort)Keys.End,
        ["pgup"] = (ushort)Keys.PageUp,
        ["pageup"] = (ushort)Keys.PageUp,
        ["pgdn"] = (ushort)Keys.PageDown,
        ["pagedown"] = (ushort)Keys.PageDown,
        ["up"] = (ushort)Keys.Up,
        ["down"] = (ushort)Keys.Down,
        ["left"] = (ushort)Keys.Left,
        ["right"] = (ushort)Keys.Right,
        ["backspace"] = (ushort)Keys.Back,
        ["prtsc"] = (ushort)Keys.PrintScreen
    };

    public static bool TryParse(string? text, out ushort[] modifiers, out ushort key)
    {
        modifiers = Array.Empty<ushort>();
        key = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var tokens = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var mods = new List<ushort>();
        string? keyToken = null;

        foreach (var token in tokens)
        {
            switch (token.ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    mods.Add(NativeMethods.VK_CONTROL);
                    break;
                case "shift":
                    mods.Add(NativeMethods.VK_SHIFT);
                    break;
                case "alt":
                    mods.Add(NativeMethods.VK_MENU);
                    break;
                case "win":
                case "super":
                    mods.Add(NativeMethods.VK_LWIN);
                    break;
                default:
                    if (keyToken is not null) return false;
                    keyToken = token;
                    break;
            }
        }

        if (keyToken is null || !TryKey(keyToken, out key)) return false;
        modifiers = mods.ToArray();
        return true;
    }

    private static bool TryKey(string token, out ushort vk)
    {
        if (Aliases.TryGetValue(token, out vk)) return true;
        if (Enum.TryParse<Keys>(token, true, out var parsed))
        {
            vk = (ushort)parsed;
            return true;
        }
        vk = 0;
        return false;
    }
}
