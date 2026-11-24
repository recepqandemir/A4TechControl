using Microsoft.Win32;

namespace A4TechCompanion;

/// <summary>"Açılışta başlat" ayarını HKCU Run anahtarında tutar.</summary>
public static class AutoStart
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "A4TechCompanion";

    public static bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
                return key?.GetValue(AppName) is not null;
            }
            catch (Exception ex)
            {
                Logger.Append(ex);
                return false;
            }
        }
    }

    public static void Set(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (key is null) return;

            if (enabled)
                key.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
            else
                key.DeleteValue(AppName, false);
        }
        catch (Exception ex)
        {
            Logger.Append(ex);
        }
    }
}
