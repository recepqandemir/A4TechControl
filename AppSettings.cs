using System.Text.Json;

namespace A4TechCompanion;

/// <summary>
/// Kullanıcı ayarları. %AppData%\A4TechCompanion\settings.json içinde saklanır.
/// Eksik/bozuk dosya sessizce varsayılanlara düşer; uygulama asla çökmez.
/// </summary>
public sealed class AppSettings
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "A4TechCompanion");
    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    public static AppSettings Current { get; private set; } = Load();

    public bool AutoLaunchOnBoot { get; set; } = true;
    public bool SwapSideButtons { get; set; }
    public bool EnableFourWayWheel { get; set; } = true;
    public bool EnableGestures { get; set; } = true;
    public bool ShowGestureOverlay { get; set; } = true;
    public bool EnableFnMapping { get; set; } = true;
    public string MiddleButtonAction { get; set; } = "None";
    public string Button4Action { get; set; } = "VolumeDown";
    public string Button5Action { get; set; } = "VolumeUp";
    public Dictionary<string, string> GestureActions { get; set; } = GestureNames.Defaults();
    public Dictionary<string, int> FnTriggers { get; set; } = FnKeys.DefaultTriggers();
    public Dictionary<string, string> FnActions { get; set; } = FnKeys.DefaultActions();

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            settings.GestureActions ??= GestureNames.Defaults();

            var defaults = GestureNames.Defaults();
            foreach (var direction in GestureNames.All)
                if (!settings.GestureActions.ContainsKey(direction))
                    settings.GestureActions[direction] = defaults[direction];

            settings.FnTriggers ??= FnKeys.DefaultTriggers();
            settings.FnActions ??= FnKeys.DefaultActions();
            var defaultTriggers = FnKeys.DefaultTriggers();
            foreach (var slot in FnKeys.All)
            {
                if (!settings.FnTriggers.ContainsKey(slot))
                    settings.FnTriggers[slot] = defaultTriggers[slot];
                if (!settings.FnActions.ContainsKey(slot))
                    settings.FnActions[slot] = "None";
            }

            return settings;
        }
        catch (Exception ex)
        {
            Logger.Append(ex);
            return new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, options));
        }
        catch (Exception ex)
        {
            Logger.Append(ex);
        }
    }
}
