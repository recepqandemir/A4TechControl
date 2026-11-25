using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static A4TechControl.NativeMethods;

namespace A4TechControl;

/// <summary>
/// Tüm sistem fare olaylarını WH_MOUSE_LL ile izler:
/// - Yan tuşları (4/5) ses/kısayol eylemlerine çevirir
/// - Sağ tuş basılı hareketini jest olarak tanır
/// - 4 yönlü tekerleği açıp kapatır
/// </summary>
public sealed class MouseHook : IDisposable
{
    private IntPtr _handle;
    private HookProc? _proc;
    private bool _side4Consumed;
    private bool _side5Consumed;
    private bool _middleConsumed;
    private bool _gestureTracking;
    private bool _gestureMoved;
    private long _gestureStartTime;
    private POINT _gestureStart;

    private const long GestureHoldMs = 200;

    private readonly System.Windows.Forms.Timer _repeatTimer = new() { Interval = 350 };
    private string? _repeatAction;

    public MouseHook()
    {
        _repeatTimer.Tick += (_, _) =>
        {
            if (_repeatAction is null) return;
            ActionDispatcher.Execute(_repeatAction);
            _repeatTimer.Interval = 120;   // ilk gecikmeden sonra klavye tekrarı hızı
        };
    }

    /// <summary>Jest çizilirken ekran geri bildirimini çizen katman (isteğe bağlı).</summary>
    public IGestureFeedback? Feedback { get; set; }

    public void Install()
    {
        if (_handle != IntPtr.Zero) return;
        _proc = HookCallback;
        _handle = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(null), 0);
        if (_handle == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0)
            {
                var data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                if (data.dwExtraInfo.ToUInt64() != InputSender.Tag && Handle((int)wParam, data))
                    return new IntPtr(1);
            }
        }
        catch (Exception ex)
        {
            // Hook içinde fırlatılan bir istisna tüm sistemi etkileyebilir; asla dışarı sızmasın.
            Logger.Append(ex);
        }
        return CallNextHookEx(_handle, nCode, wParam, lParam);
    }

    private bool Handle(int msg, MSLLHOOKSTRUCT data)
    {
        var s = AppSettings.Current;

        switch (msg)
        {
            case WM_MBUTTONDOWN:
            {
                var action = s.MiddleButtonAction;
                if (action.Equals("None", StringComparison.OrdinalIgnoreCase))
                    return false;
                ActionDispatcher.Execute(action);
                _middleConsumed = true;
                StartRepeat(action);
                return true;
            }

            case WM_MBUTTONUP:
                if (_middleConsumed)
                {
                    _middleConsumed = false;
                    StopRepeatIfIdle();
                    return true;
                }
                return false;

            case WM_XBUTTONDOWN:
            {
                uint button = data.mouseData >> 16;
                bool isSide4 = button == XBUTTON1;
                if (!isSide4 && button != XBUTTON2) return false;

                string action = ResolveSideAction(s, isSide4);
                bool consumed = false;

                if (action.Equals("None", StringComparison.OrdinalIgnoreCase))
                {
                    // "Takas" açıksa, eylem atanmamış olsa bile fiziksel tuşları değiştir.
                    if (s.SwapSideButtons)
                    {
                        InputSender.TapMouseX(isSide4 ? XBUTTON2 : XBUTTON1);
                        consumed = true;
                    }
                }
                else
                {
                    ActionDispatcher.Execute(action);
                    StartRepeat(action);
                    consumed = true;
                }

                if (consumed)
                {
                    VolumeEchoGuard.SideButtonHeld = true;
                    if (isSide4) _side4Consumed = true;
                    else _side5Consumed = true;
                }
                return consumed;
            }

            case WM_XBUTTONUP:
            {
                uint button = data.mouseData >> 16;
                bool consumed = false;
                if (button == XBUTTON1 && _side4Consumed)
                {
                    _side4Consumed = false;
                    consumed = true;
                }
                if (button == XBUTTON2 && _side5Consumed)
                {
                    _side5Consumed = false;
                    consumed = true;
                }
                if (consumed) StopRepeatIfIdle();
                return consumed;
            }

            case WM_RBUTTONDOWN:
                if (s.EnableGestures)
                {
                    _gestureTracking = true;
                    _gestureMoved = false;
                    _gestureStartTime = Environment.TickCount64;
                    _gestureStart = data.pt;
                }
                return false;

            case WM_MOUSEMOVE:
                if (_gestureTracking)
                {
                    int dx = data.pt.X - _gestureStart.X;
                    int dy = data.pt.Y - _gestureStart.Y;
                    int limit = GestureRecognizer.MinimumDistance;
                    if (dx * dx + dy * dy >= limit * limit)
                        _gestureMoved = true;

                    bool heldLongEnough = Environment.TickCount64 - _gestureStartTime >= GestureHoldMs;
                    if (_gestureMoved && heldLongEnough && s.ShowGestureOverlay && Feedback is not null)
                    {
                        var liveDirection = GestureRecognizer.GetDirection(_gestureStart, data.pt);
                        var liveAction = liveDirection is not null && s.GestureActions.TryGetValue(liveDirection, out var a)
                            ? a
                            : null;
                        Feedback.Update(_gestureStart, data.pt, liveDirection, liveAction);
                    }
                }
                return false;

            case WM_RBUTTONUP:
                Feedback?.Hide();
                if (_gestureTracking)
                {
                    _gestureTracking = false;
                    bool heldLongEnough = Environment.TickCount64 - _gestureStartTime >= GestureHoldMs;
                    if (_gestureMoved && heldLongEnough)
                    {
                        _gestureMoved = false;
                        var direction = GestureRecognizer.GetDirection(_gestureStart, data.pt);
                        if (direction is not null &&
                            s.GestureActions.TryGetValue(direction, out var action) &&
                            !action.Equals("None", StringComparison.OrdinalIgnoreCase))
                        {
                            ActionDispatcher.Execute(action);
                        }
                        // Jest kullanıldıysa sağ tık menüsünü açmayı engelle.
                        return true;
                    }
                }
                return false;

            case WM_MOUSEHWHEEL:
                return !s.EnableFourWayWheel;

            default:
                return false;
        }
    }

    private void StartRepeat(string action)
    {
        _repeatAction = action;
        _repeatTimer.Interval = 350;
        _repeatTimer.Start();
    }

    private void StopRepeatIfIdle()
    {
        if (_side4Consumed || _side5Consumed || _middleConsumed) return;
        _repeatTimer.Stop();
        _repeatAction = null;
        VolumeEchoGuard.SideButtonHeld = false;
    }

    private static string ResolveSideAction(AppSettings s, bool isSide4)
    {
        if (s.SwapSideButtons)
            return isSide4 ? s.Button5Action : s.Button4Action;
        return isSide4 ? s.Button4Action : s.Button5Action;
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_handle);
            _handle = IntPtr.Zero;
        }
        _proc = null;
        _repeatTimer.Stop();
        _repeatTimer.Dispose();
    }
}
