using System.IO;
using FSModLauncher.Models;
using Newtonsoft.Json;

namespace FSModLauncher.Services;

public class ConfigService
{
    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(AppPaths.SettingsFile))
            {
                var defaultSettings = new AppSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(AppPaths.SettingsFile);
            return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(AppPaths.ConfigDirectory);
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await File.WriteAllTextAsync(AppPaths.SettingsFile, json);
        }
        catch
        {
            // Log error in real implementation
        }
    }

    public string GetLogsPath()
    {
        return AppPaths.LogsDirectory;
    }
}