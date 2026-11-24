namespace A4TechControl;

/// <summary>En küçük hata günlüğü. Sadece istisnaları yazar, normal çalışmayı yavaşlatmaz.</summary>
public static class Logger
{
    private static readonly object Sync = new();
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "A4TechControl", "error.log");

    public static void Append(Exception ex)
    {
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);
                File.AppendAllText(Path,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\r\n");
            }
        }
        catch
        {
            // Günlük yazılamıyorsa uygulamayı etkilemesin.
        }
    }
}
