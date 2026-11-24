using System.Drawing;
using System.Windows.Forms;

namespace A4TechControl;

/// <summary>Sistem tepsisinde yaşayan uygulama. Ana form yoktur.</summary>
public sealed class TrayAppContext : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly ContextMenuStrip _menu = new();
    private readonly MouseHook _mouseHook = new();
    private readonly GestureOverlay _overlay = new();
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _swapItem;
    private readonly ToolStripMenuItem _gestureItem;
    private readonly ToolStripMenuItem _overlayItem;
    private readonly ToolStripMenuItem _fnItem;
    private readonly ToolStripMenuItem _wheelItem;
    private readonly ToolStripMenuItem _autoStartItem;
    private readonly ToolStripMenuItem _versionItem;
    private readonly ToolStripMenuItem _exitItem;
    private SettingsForm? _settingsForm;
    private bool _idleHooked;

    public TrayAppContext()
    {
        _settingsItem = new ToolStripMenuItem("Ayarlar...");
        _swapItem = new ToolStripMenuItem("4. ve 5. tuş takası");
        _gestureItem = new ToolStripMenuItem("Jestler");
        _overlayItem = new ToolStripMenuItem("Jest okunu göster");
        _fnItem = new ToolStripMenuItem("Fn tuş eşlemeleri");
        _wheelItem = new ToolStripMenuItem("4 yönlü tekerlek");
        _autoStartItem = new ToolStripMenuItem("Açılışta başlat");
        _versionItem = new ToolStripMenuItem($"Sürüm {AppVersion.Display}") { Enabled = false };
        _exitItem = new ToolStripMenuItem("Çıkış");

        _settingsItem.Click += (_, _) => OpenSettings();
        _swapItem.Click += (_, _) => Toggle(s => s.SwapSideButtons = !s.SwapSideButtons);
        _gestureItem.Click += (_, _) => Toggle(s => s.EnableGestures = !s.EnableGestures);
        _overlayItem.Click += (_, _) => Toggle(s => s.ShowGestureOverlay = !s.ShowGestureOverlay);
        _fnItem.Click += (_, _) => Toggle(s => s.EnableFnMapping = !s.EnableFnMapping);
        _wheelItem.Click += (_, _) => Toggle(s => s.EnableFourWayWheel = !s.EnableFourWayWheel);
        _autoStartItem.Click += (_, _) =>
        {
            var s = AppSettings.Current;
            s.AutoLaunchOnBoot = !s.AutoLaunchOnBoot;
            AutoStart.Set(s.AutoLaunchOnBoot);
            s.Save();
        };
        _exitItem.Click += (_, _) => Application.Exit();

        _menu.Items.AddRange(new ToolStripItem[]
        {
            _settingsItem,
            new ToolStripSeparator(),
            _swapItem,
            _gestureItem,
            _overlayItem,
            _fnItem,
            _wheelItem,
            new ToolStripSeparator(),
            _autoStartItem,
            new ToolStripSeparator(),
            _versionItem,
            _exitItem
        });
        _menu.Renderer = new ModernMenuRenderer();
        _menu.Font = new Font("Segoe UI", 9F);
        _menu.Opening += (_, _) => RefreshChecks();

        _tray = new NotifyIcon
        {
            Icon = TrayIconFactory.Create(),
            Text = "A4Tech Control (FG35 / FGK10) — Ayarlar için çift tıkla",
            Visible = false,
            ContextMenuStrip = _menu
        };
        _tray.DoubleClick += (_, _) => OpenSettings();

        _mouseHook.Feedback = _overlay;
        _mouseHook.Install();
        KeyboardHook.Instance.Install();

        // İkonu mesaj döngüsü başladıktan sonra göster; Explorer hazır olmadan
        // görünmeyen ikon sorununu önler. İlk açılışta balon ile haber ver.
        Application.Idle += OnFirstIdle;
        _idleHooked = true;
    }

    private void OnFirstIdle(object? sender, EventArgs e)
    {
        if (!_idleHooked) return;
        _idleHooked = false;
        Application.Idle -= OnFirstIdle;

        _tray.Visible = true;
        _tray.ShowBalloonTip(4000,
            "A4Tech Control",
            "FG35 / FGK10 hazır. Ayarlar için tepsi ikonuna çift tıkla.",
            ToolTipIcon.Info);
    }

    private static void Toggle(Action<AppSettings> change)
    {
        var s = AppSettings.Current;
        change(s);
        s.Save();
    }

    private void RefreshChecks()
    {
        var s = AppSettings.Current;
        _swapItem.Checked = s.SwapSideButtons;
        _gestureItem.Checked = s.EnableGestures;
        _overlayItem.Checked = s.ShowGestureOverlay;
        _fnItem.Checked = s.EnableFnMapping;
        _wheelItem.Checked = s.EnableFourWayWheel;
        _autoStartItem.Checked = AutoStart.IsEnabled;
    }

    private void OpenSettings()
    {
        if (_settingsForm is null || _settingsForm.IsDisposed)
            _settingsForm = new SettingsForm();
        _settingsForm.Show();
        _settingsForm.BringToFront();
    }

    protected override void ExitThreadCore()
    {
        if (_idleHooked)
        {
            _idleHooked = false;
            Application.Idle -= OnFirstIdle;
        }
        _tray.Visible = false;
        _tray.Dispose();
        _menu.Dispose();
        _overlay.Hide();
        _overlay.Dispose();
        _mouseHook.Dispose();
        KeyboardHook.Instance.Dispose();
        base.ExitThreadCore();
    }
}
