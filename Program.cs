using System.Threading;

namespace A4TechCompanion;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Aynı anda birden fazla kopya çalışmasın (çift hook = çift eylem).
        using var mutex = new Mutex(true, "A4TechCompanion_8F3C2A11", out bool createdNew);
        if (!createdNew) return;

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
