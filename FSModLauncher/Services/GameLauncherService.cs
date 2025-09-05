using System.Diagnostics;
using System.IO;

namespace FSModLauncher.Services;

public class GameLauncherService
{
    public Task<bool> LaunchGameAsync(string gameExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(gameExePath) || !File.Exists(gameExePath))
                throw new FileNotFoundException("Game executable not found or path is empty.");

            var startInfo = new ProcessStartInfo
            {
                FileName = gameExePath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(gameExePath)
            };

            var process = Process.Start(startInfo);
            return Task.FromResult(process != null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to launch game: {ex.Message}", ex);
        }
    }
}