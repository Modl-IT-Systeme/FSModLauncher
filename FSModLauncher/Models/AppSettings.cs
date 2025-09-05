using System.IO;

namespace FSModLauncher.Models;

public class AppSettings
{
    public string ModsFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Documents", "My Games", "FarmingSimulator25", "mods");

    public string GameExePath { get; set; } = "";
    public string ServerIp { get; set; } = "192.168.178.35";
    public string ServerPort { get; set; } = "8080";
    public string ServerCode { get; set; } = "8bedd3c7441fa9333f463c568f2d52f2";
    public string HashAlgorithm { get; set; } = "MD5";
    public int ConcurrentDownloads { get; set; } = 3;
    public bool BackupBeforeOverwrite { get; set; } = true;

    public string GetServerStatsUrl()
    {
        return $"http://{ServerIp}:{ServerPort}/feed/dedicated-server-stats.xml?code={ServerCode}";
    }

    public string GetCdnBaseUrl()
    {
        return $"http://{ServerIp}:{ServerPort}/mods";
    }
}