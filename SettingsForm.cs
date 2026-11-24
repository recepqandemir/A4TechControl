using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace A4TechControl;

public sealed class SettingsForm : Form
{
    [DllImport("user32.dll")] private static extern bool ReleaseCapture();
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    private const uint WmNcLButtonDown = 0x00A1;
    private const int HtCaption = 2;

    private void HeaderDrag(object? sender, MouseEventArgs e)
    {
        ReleaseCapture();
        SendMessage(Handle, WmNcLButtonDown, new IntPtr(HtCaption), IntPtr.Zero);
    }

    private static ComboBox NewCombo()
    {
        var combo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDown,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Input,
            ForeColor = Theme.Text,
            Width = 210,
            Font = new Font("Segoe UI", 9F),
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 20
        };
        combo.DrawItem += (sender, e) =>
        {
            if (e.Index < 0) return;
            bool selected = e.State.HasFlag(DrawItemState.Selected);
            using var brush = new SolidBrush(selected ? Theme.Accent : Theme.Input);
            e.Graphics.FillRectangle(brush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, combo.Items[e.Index]?.ToString() ?? string.Empty,
                combo.Font, e.Bounds, selected ? Theme.Window : Theme.Text,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        };
        return combo;
    }

    private static Label NewLabel(string key, int x, int y, int w = 150, bool muted = false) => new()
    {
        Text = Lang.T(key),
        Tag = key,
        Location = new Point(x, y),
        Size = new Size(w, 20),
        ForeColor = muted ? Theme.Muted : Theme.Text,
        Font = new Font("Segoe UI", 9F)
    };

    private readonly ComboBox _button3 = NewCombo();
    private readonly ComboBox _button4 = NewCombo();
    private readonly ComboBox _button5 = NewCombo();
    private readonly ModernCheckBox _swap = new() { Text = "4. ve 5. tuşları takas et", Tag = "chk.swap", Size = new Size(380, 20) };
    private readonly ModernCheckBox _gesturesEnabled = new() { Text = "Jestleri etkinleştir (sağ tuş basılı tut + hareket)", Tag = "chk.gestures", Size = new Size(470, 20) };
    private readonly ModernCheckBox _overlayEnabled = new() { Text = "Jest sırasında ekranda ok göster", Tag = "chk.overlay", Size = new Size(400, 20) };
    private readonly ModernCheckBox _fnEnabled = new() { Text = "Fn tuş eşlemelerini etkinleştir", Tag = "chk.fn", Size = new Size(400, 20) };
    private readonly ModernCheckBox _wheelEnabled = new() { Text = "4 yönlü tekerleği etkinleştir", Tag = "chk.wheel", Size = new Size(400, 20) };
    private readonly ModernCheckBox _autoStart = new() { Text = "Windows ile birlikte başlat", Tag = "chk.autostart", Size = new Size(400, 20) };
    private readonly Label _status = new() { Text = "", ForeColor = Theme.Muted, Font = new Font("Segoe UI", 9F) };
    private readonly Label _versionLabel = new() { ForeColor = Theme.Muted, Font = new Font("Segoe UI", 9F) };
    private readonly ModernButton _trButton = new() { Text = "TR", Size = new Size(34, 28), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
    private readonly ModernButton _enButton = new() { Text = "EN", Size = new Size(34, 28), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
    private readonly Dictionary<string, ComboBox> _gestureCombos = new();
    private readonly Dictionary<string, ComboBox> _fnCombos = new();
    private readonly Dictionary<string, ToolTip> _fnTips = new();
    private readonly Dictionary<string, Control> _fnGroupControls = new();
    private string? _capturingSlot;

    public SettingsForm()
    {
        Text = "A4Tech Control — FG35 / FGK10";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = true;
        KeyPreview = true;
        ClientSize = new Size(540, 940);
        BackColor = Theme.Window;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 9F);
        AutoScroll = true;
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) Close(); };

        BuildUi();
        LoadSettings();
        ApplyLanguage();

        KeyboardHook.Instance.KeyCaptured += OnKeyCaptured;
        Lang.Changed += ApplyLanguage;
        Disposed += (_, _) =>
        {
            KeyboardHook.Instance.KeyCaptured -= OnKeyCaptured;
            Lang.Changed -= ApplyLanguage;
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Theme.Border);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    private ModernPanel AddCard(string title, int y, int height)
    {
        var card = new ModernPanel { Location = new Point(16, y), Size = new Size(508, height) };
        card.Controls.Add(new Label
        {
            Text = Lang.T(title),
            Tag = title,
            Location = new Point(16, 11),
            Size = new Size(470, 20),
            ForeColor = Theme.Accent,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        });
        Controls.Add(card);
        return card;
    }

    private void ApplyLanguage()
    {
        ApplyText(this);
        _versionLabel.Text = Lang.F("version.label", AppVersion.Display);
        _trButton.IsAccent = Lang.Current == Lang.Turkish;
        _enButton.IsAccent = Lang.Current == Lang.English;
        _trButton.Invalidate();
        _enButton.Invalidate();
    }

    private static void ApplyText(Control parent)
    {
        foreach (Control control in parent.Controls)
        {
            if (control.Tag is string tag && tag.Length > 0)
            {
                control.Text = tag.StartsWith("gesturelabel.", StringComparison.Ordinal)
                    ? GestureNames.Display(tag["gesturelabel.".Length..])
                    : Lang.T(tag);
            }
            if (control.HasChildren) ApplyText(control);
        }
    }

    private void BuildUi()
    {
        // --- Başlık ---
        var header = new Panel { Location = new Point(0, 0), Size = new Size(540, 72), BackColor = Theme.Window };
        header.Paint += (_, e) =>
        {
            e.Graphics.Clear(Theme.Window);
            using var pen = new Pen(Theme.Accent, 3);
            e.Graphics.DrawLine(pen, 0, 69, 540, 69);
        };
        header.Controls.Add(new Label
        {
            Text = "A4Tech Control",
            Location = new Point(20, 12),
            Size = new Size(360, 30),
            ForeColor = Theme.Text,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold)
        });
        header.Controls.Add(new Label
        {
            Text = "FG35 / FGK10  •  Windows 11",
            Location = new Point(21, 44),
            Size = new Size(360, 18),
            ForeColor = Theme.Muted,
            Font = new Font("Segoe UI", 9F)
        });
        _trButton.Location = new Point(374, 18);
        _enButton.Location = new Point(414, 18);
        _trButton.Click += (_, _) => Lang.Set(Lang.Turkish);
        _enButton.Click += (_, _) => Lang.Set(Lang.English);
        header.Controls.Add(_trButton);
        header.Controls.Add(_enButton);

        var minimize = new ModernButton { Text = "—", Location = new Point(466, 18), Size = new Size(28, 28), Font = new Font("Segoe UI", 10F) };
        minimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
        var close = new ModernButton { Text = "✕", Location = new Point(500, 18), Size = new Size(28, 28), Font = new Font("Segoe UI", 10F) };
        close.Click += (_, _) => Close();
        header.Controls.Add(minimize);
        header.Controls.Add(close);
        header.MouseDown += HeaderDrag;
        foreach (Control child in header.Controls)
            if (child is not Button) child.MouseDown += HeaderDrag;
        Controls.Add(header);

        // --- Fare tuşları ---
        var buttons = AddCard("card.mouse", 84, 156);
        buttons.Controls.Add(NewLabel("lbl.button3", 16, 40));
        buttons.Controls.Add(NewLabel("lbl.button4", 16, 70));
        buttons.Controls.Add(NewLabel("lbl.button5", 16, 100));
        _button3.Location = new Point(170, 37);
        _button4.Location = new Point(170, 67);
        _button5.Location = new Point(170, 97);
        _swap.Location = new Point(16, 130);
        buttons.Controls.Add(_button3);
        buttons.Controls.Add(_button4);
        buttons.Controls.Add(_button5);
        buttons.Controls.Add(_swap);

        // --- Jestler ---
        var gestures = AddCard("card.gestures", 252, 258);
        _gesturesEnabled.Location = new Point(16, 40);
        gestures.Controls.Add(_gesturesEnabled);
        for (int i = 0; i < GestureNames.All.Length; i++)
        {
            var direction = GestureNames.All[i];
            int column = i / 8;
            int row = i % 8;
            int labelX = column == 0 ? 16 : 270;
            int comboX = column == 0 ? 130 : 384;

            var label = NewLabel("gesturelabel." + direction, labelX, 66 + row * 22, 110, true);
            label.Font = new Font("Segoe UI", 8F);
            gestures.Controls.Add(label);

            var combo = NewCombo();
            combo.Width = 110;
            combo.Font = new Font("Segoe UI", 8F);
            combo.Location = new Point(comboX, 63 + row * 22);
            _gestureCombos[direction] = combo;
            gestures.Controls.Add(combo);
        }

        // --- Genel ---
        var general = AddCard("card.general", 522, 136);
        _wheelEnabled.Location = new Point(16, 38);
        _autoStart.Location = new Point(16, 62);
        _overlayEnabled.Location = new Point(16, 86);
        _fnEnabled.Location = new Point(16, 110);
        general.Controls.Add(_wheelEnabled);
        general.Controls.Add(_autoStart);
        general.Controls.Add(_overlayEnabled);
        general.Controls.Add(_fnEnabled);

        // --- Fn tuşları ---
        var fn = AddCard("card.fn", 670, 190);
        var fnFont = new Font("Segoe UI", 8F);
        for (int i = 0; i < FnKeys.All.Length; i++)
        {
            var slot = FnKeys.All[i];
            int column = i / 6;
            int row = i % 6;
            int labelX = column == 0 ? 16 : 248;
            int comboX = column == 0 ? 74 : 306;
            int buttonX = column == 0 ? 180 : 412;

            var label = NewLabel(FnKeys.Display(slot), labelX, 46 + row * 22, 58, true);
            label.Font = fnFont;
            fn.Controls.Add(label);

            var combo = NewCombo();
            combo.Width = 100;
            combo.Font = fnFont;
            combo.Location = new Point(comboX, 43 + row * 22);
            _fnCombos[slot] = combo;
            fn.Controls.Add(combo);

            var learn = new ModernButton
            {
                Text = Lang.T("btn.learn"),
                Tag = "btn.learn",
                Location = new Point(buttonX, 41 + row * 22),
                Size = new Size(48, 22),
                Font = fnFont
            };
            learn.Click += (_, _) => StartCapture(slot);
            _fnGroupControls[slot] = learn;
            var tip = new ToolTip { BackColor = Theme.Card, ForeColor = Theme.Text };
            _fnTips[slot] = tip;
            tip.SetToolTip(learn, "Öğrenmek için tıkla, sonra klavyedeki tuşa bas.");
            fn.Controls.Add(learn);
        }

        _status.Location = new Point(20, 874);
        _status.Size = new Size(480, 20);
        Controls.Add(_status);

        _versionLabel.Location = new Point(20, 902);
        _versionLabel.Size = new Size(280, 30);
        Controls.Add(_versionLabel);

        var save = new ModernButton
        {
            Text = Lang.T("btn.save"),
            Tag = "btn.save",
            Location = new Point(330, 902),
            Size = new Size(90, 32),
            IsAccent = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
        var cancel = new ModernButton
        {
            Text = Lang.T("btn.cancel"),
            Tag = "btn.cancel",
            Location = new Point(430, 902),
            Size = new Size(90, 32),
            Font = new Font("Segoe UI", 9.5F)
        };
        save.Click += Save_Click;
        cancel.Click += (_, _) => Close();
        Controls.Add(save);
        Controls.Add(cancel);

        CancelButton = cancel;
    }

    private void LoadSettings()
    {
        var s = AppSettings.Current;
        foreach (var combo in new[] { _button3, _button4, _button5 }.Concat(_gestureCombos.Values).Concat(_fnCombos.Values))
        {
            combo.Items.Clear();
            combo.Items.AddRange(ActionDispatcher.Choices);
        }

        _button3.Text = s.MiddleButtonAction;
        _button4.Text = s.Button4Action;
        _button5.Text = s.Button5Action;
        _swap.Checked = s.SwapSideButtons;
        _gesturesEnabled.Checked = s.EnableGestures;
        _overlayEnabled.Checked = s.ShowGestureOverlay;
        _fnEnabled.Checked = s.EnableFnMapping;
        _wheelEnabled.Checked = s.EnableFourWayWheel;
        _autoStart.Checked = AutoStart.IsEnabled;

        foreach (var (direction, combo) in _gestureCombos)
            combo.Text = s.GestureActions.GetValueOrDefault(direction, "None");

        foreach (var slot in FnKeys.All)
        {
            _fnCombos[slot].Text = s.FnActions.GetValueOrDefault(slot, "None");
            var vk = s.FnTriggers.GetValueOrDefault(slot, 0);
            if (_fnGroupControls.TryGetValue(slot, out var control))
                _fnTips[slot].SetToolTip(control,
                    $"Tetikleyici: {FnKeys.KeyName(vk)} (0x{vk:X2})\nDeğiştirmek için tıkla, sonra tuşa bas.");
        }
    }

    private void StartCapture(string slot)
    {
        _capturingSlot = slot;
        KeyboardHook.Instance.CaptureNext = true;
        _status.Text = Lang.F("status.capturing", FnKeys.Display(slot));
    }

    private void OnKeyCaptured(int vk)
    {
        if (_capturingSlot is null) return;
        var slot = _capturingSlot;
        _capturingSlot = null;

        if (vk < 0)
        {
            _status.Text = Lang.T("status.cancelled");
            return;
        }

        AppSettings.Current.FnTriggers[slot] = vk;
        _status.Text = Lang.F("status.learned", FnKeys.Display(slot), FnKeys.KeyName(vk), vk);
        if (_fnGroupControls.TryGetValue(slot, out var control))
            _fnTips[slot].SetToolTip(control,
                $"Tetikleyici: {FnKeys.KeyName(vk)} (0x{vk:X2})\nDeğiştirmek için tıkla, sonra tuşa bas.");
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        var values = new List<(string Name, string Value)>
        {
            ("Tuş 3", _button3.Text),
            ("Tuş 4", _button4.Text),
            ("Tuş 5", _button5.Text)
        };
        values.AddRange(_gestureCombos.Select(kv => (GestureNames.Display(kv.Key), kv.Value.Text)));
        values.AddRange(_fnCombos.Select(kv => (FnKeys.Display(kv.Key), kv.Value.Text)));

        foreach (var (name, value) in values)
        {
            if (!ActionDispatcher.IsValid(value))
            {
                MessageBox.Show(this,
                    Lang.F("validation.message", name, value),
                    Lang.T("validation.title"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        var s = AppSettings.Current;
        s.MiddleButtonAction = _button3.Text.Trim();
        s.Button4Action = _button4.Text.Trim();
        s.Button5Action = _button5.Text.Trim();
        s.SwapSideButtons = _swap.Checked;
        s.EnableGestures = _gesturesEnabled.Checked;
        s.ShowGestureOverlay = _overlayEnabled.Checked;
        s.EnableFnMapping = _fnEnabled.Checked;
        s.EnableFourWayWheel = _wheelEnabled.Checked;
        s.AutoLaunchOnBoot = _autoStart.Checked;
        foreach (var (direction, combo) in _gestureCombos)
            s.GestureActions[direction] = combo.Text.Trim();
        foreach (var (slot, combo) in _fnCombos)
            s.FnActions[slot] = combo.Text.Trim();

        s.Save();
        AutoStart.Set(s.AutoLaunchOnBoot);
        Close();
    }
}
