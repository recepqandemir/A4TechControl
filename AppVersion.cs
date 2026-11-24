using System.Reflection;

namespace A4TechControl;

/// <summary>Derleme sürümü (csproj içindeki Version etiketinden gelir).</summary>
public static class AppVersion
{
    public static string Display { get; } =
        typeof(AppVersion).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
}
