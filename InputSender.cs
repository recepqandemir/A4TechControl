using System.Runtime.InteropServices;

namespace A4TechControl;

/// <summary>
/// SendInput ile yapay klavye/fare olayları gönderir.
/// Gönderilen olaylar özel bir etiket (dwExtraInfo) taşır; hook bunları görüp
/// tekrar işlemez, böylece sonsuz döngü oluşmaz.
/// </summary>
public static class InputSender
{
    public const ulong Tag = 0x41415343; // "AASC"
    private static readonly int InputSize = Marshal.SizeOf<NativeMethods.INPUT>();

    private static NativeMethods.INPUT MakeKey(ushort vk, bool up) => new()
    {
        type = NativeMethods.INPUT_KEYBOARD,
        U = new NativeMethods.InputUnion
        {
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = vk,
                dwFlags = up ? NativeMethods.KEYEVENTF_KEYUP : 0u,
                dwExtraInfo = new UIntPtr(Tag)
            }
        }
    };

    private static NativeMethods.INPUT MakeMouse(uint flags, uint data = 0) => new()
    {
        type = NativeMethods.INPUT_MOUSE,
        U = new NativeMethods.InputUnion
        {
            mi = new NativeMethods.MOUSEINPUT
            {
                dwFlags = flags,
                mouseData = data,
                dwExtraInfo = new UIntPtr(Tag)
            }
        }
    };

    private static void Send(params NativeMethods.INPUT[] inputs)
    {
        if (inputs.Length == 0) return;
        NativeMethods.SendInput((uint)inputs.Length, inputs, InputSize);
    }

    public static void TapKey(ushort vk) => Send(MakeKey(vk, false), MakeKey(vk, true));

    public static void TapMouseRight() =>
        Send(MakeMouse(NativeMethods.MOUSEEVENTF_RIGHTDOWN),
             MakeMouse(NativeMethods.MOUSEEVENTF_RIGHTUP));

    public static void TapMouseMiddle() =>
        Send(MakeMouse(NativeMethods.MOUSEEVENTF_MIDDLEDOWN),
             MakeMouse(NativeMethods.MOUSEEVENTF_MIDDLEUP));

    public static void TapMouseX(uint xButton)
    {
        Send(MakeMouse(NativeMethods.MOUSEEVENTF_XDOWN, xButton),
             MakeMouse(NativeMethods.MOUSEEVENTF_XUP, xButton));
    }

    /// <summary>Ctrl+Shift+A gibi bir kısayolu basma/bırakma sırasıyla gönderir.</summary>
    public static void SendHotkey(ushort[] modifiers, ushort key)
    {
        var inputs = new List<NativeMethods.INPUT>(modifiers.Length * 2 + 2);
        foreach (var mod in modifiers) inputs.Add(MakeKey(mod, false));
        inputs.Add(MakeKey(key, false));
        inputs.Add(MakeKey(key, true));
        for (int i = modifiers.Length - 1; i >= 0; i--) inputs.Add(MakeKey(modifiers[i], true));
        Send(inputs.ToArray());
    }
}
