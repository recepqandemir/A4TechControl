using System.ComponentModel;
using System.Runtime.InteropServices;
using static A4TechCompanion.NativeMethods;

namespace A4TechCompanion;

/// <summary>
/// WH_KEYBOARD_LL ile Fn tuşlarının gönderdiği multimedia kodlarını yakalar.
/// Ayarlar penceresindeki "Öğren" düğmesi CaptureNext ile tek seferlik kayıt yapar.
/// </summary>
public sealed class KeyboardHook : IDisposable
{
    private static readonly Lazy<KeyboardHook> LazyInstance = new(() => new KeyboardHook());
    public static KeyboardHook Instance => LazyInstance.Value;

    private IntPtr _handle;
    private HookProc? _proc;

    public bool CaptureNext { get; set; }

    /// <summary>-1 = Escape ile iptal, aksi halde yakalanan sanal tuş kodu.</summary>
    public event Action<int>? KeyCaptured;

    private KeyboardHook() { }

    public void Install()
    {
        if (_handle != IntPtr.Zero) return;
        _proc = Callback;
        _handle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);
        if (_handle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                int vk = (int)data.vkCode;
                bool isKeyDown = (int)wParam is WM_KEYDOWN or WM_SYSKEYDOWN;

                if (isKeyDown && data.dwExtraInfo.ToUInt64() != InputSender.Tag)
                {
                    if (CaptureNext)
                    {
                        CaptureNext = false;
                        KeyCaptured?.Invoke(vk == VK_ESCAPE ? -1 : vk);
                        return new IntPtr(1);
                    }

                    var settings = AppSettings.Current;
                    if (settings.EnableFnMapping)
                    {
                        foreach (var slot in FnKeys.All)
                        {
                            if (!settings.FnTriggers.TryGetValue(slot, out var trigger) || trigger != vk)
                                continue;
                            if (!settings.FnActions.TryGetValue(slot, out var action))
                                continue;
                            if (action.Equals("None", StringComparison.OrdinalIgnoreCase))
                                continue;

                            ActionDispatcher.Execute(action);
                            return new IntPtr(1);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Append(ex);
        }
        return CallNextHookEx(_handle, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_handle);
            _handle = IntPtr.Zero;
        }
        _proc = null;
    }
}
