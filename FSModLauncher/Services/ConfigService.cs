using System.IO;
using FSModLauncher.Models;
using Newtonsoft.Json;

namespace FSModLauncher.Services;

public class ConfigService
{
    private readonly string _configDir;
    private readonly string _configPath;

    public ConfigService()
    {
        _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Fs25ModLauncher");
        _configPath = Path.Combine(_configDir, "settings.json");
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                var defaultSettings = new AppSettings();
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_configPath);
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
            Directory.CreateDirectory(_configDir);
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch
        {
            // Log error in real implementation
        }
    }

    public string GetLogsPath()
    {
        return Path.Combine(_configDir, "logs");
    }
}