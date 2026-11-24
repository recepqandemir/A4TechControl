using System.Threading;
using Microsoft.Win32;

namespace A4TechControl;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Aynı anda birden fazla kopya çalışmasın (çift hook = çift eylem).
        using var mutex = new Mutex(true, "A4TechControl_8F3C2A11", out bool createdNew);
        if (!createdNew) return;

        Lang.Set(AppSettings.Current.Language, persist: false);

        // Eski isimden kalan açılış kaydını temizle.
        try
        {
            using var runKey = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run", true);
            runKey?.DeleteValue("A4TechCompanion", false);
        }
        catch { }

        // İlk çalıştırmada varsayılan "Açılışta başlat" ayarını gerçekten kaydet.
        if (AppSettings.Current.AutoLaunchOnBoot && !AutoStart.IsEnabled)
            AutoStart.Set(true);

        try
        {
            Application.Run(new TrayAppContext());
        }
        catch (Exception ex)
        {
            Logger.Append(ex);
        }
    }
}
