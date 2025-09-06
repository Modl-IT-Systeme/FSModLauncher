using System.IO;

namespace FSModLauncher.Services;

public static class AppPaths
{
    private static readonly Lazy<string> _configDirectory = new(() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FSModLauncher"));

    public static string ConfigDirectory => _configDirectory.Value;

    public static string SettingsFile => Path.Combine(ConfigDirectory, "settings.json");
    
    public static string ModCacheFile => Path.Combine(ConfigDirectory, "mod_cache.json");
    
    public static string LogsDirectory => Path.Combine(ConfigDirectory, "logs");
}