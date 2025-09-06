using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using FSModLauncher.Models;

namespace FSModLauncher.Services;

public class DownloadService(int maxConcurrentDownloads = 3)
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    private readonly SemaphoreSlim _semaphore = new(maxConcurrentDownloads, maxConcurrentDownloads);

    public async Task<bool> DownloadModAsync(
        ComparisonResult modResult,
        string cdnBaseUrl,
        string modsFolder,
        string hashAlgorithm,
        bool backupBeforeOverwrite,
        IProgress<(string ModName, int ProgressPercentage, string Status)>? progress = null)
    {
        await _semaphore.WaitAsync();

        try
        {
            var modName = modResult.ServerMod.Name;
            var downloadUrl = $"{cdnBaseUrl.TrimEnd('/')}/{modName}.zip";
            var tempFilePath = Path.Combine(modsFolder, $"{modName}.zip.tmp");
            var finalFilePath = Path.Combine(modsFolder, $"{modName}.zip");

            progress?.Report((modName, 0, "Starting download..."));

            // Create mods folder if it doesn't exist
            Directory.CreateDirectory(modsFolder);

            // Backup existing file if required
            if (backupBeforeOverwrite && File.Exists(finalFilePath))
                await BackupExistingFileAsync(finalFilePath, modsFolder);

            // Download with retries
            var success = false;
            for (var attempt = 1; attempt <= 3 && !success; attempt++)
                try
                {
                    progress?.Report((modName, 0, $"Downloading (attempt {attempt})..."));

                    using var response =
                        await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    var downloadedBytes = 0L;

                    await using var contentStream = await response.Content.ReadAsStreamAsync();
                    await using var fileStream =
                        new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                    var buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                        downloadedBytes += bytesRead;

                        if (totalBytes > 0)
                        {
                            var percentage = (int)(downloadedBytes * 100 / totalBytes);
                            progress?.Report((modName, percentage, "Downloading..."));
                        }
                    }

                    success = true;
                }
                catch (Exception ex) when (attempt < 3)
                {
                    progress?.Report((modName, 0, $"Retry {attempt} failed: {ex.Message}"));
                    await Task.Delay(1000 * attempt); // Progressive delay
                }

            if (!success)
            {
                progress?.Report((modName, 0, "Download failed"));
                return false;
            }

            // Verify hash if required
            if (hashAlgorithm != "None" && !string.IsNullOrEmpty(modResult.ServerMod.Hash))
            {
                progress?.Report((modName, 100, "Verifying..."));
                var fileHash = await GiantsModHash.ComputeAsync(tempFilePath, modName);

                if (!string.Equals(fileHash, modResult.ServerMod.Hash, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(tempFilePath);
                    progress?.Report((modName, 0, "Hash verification failed"));
                    return false;
                }
            }

            // Move temp file to final location
            if (File.Exists(finalFilePath)) File.Delete(finalFilePath);
            File.Move(tempFilePath, finalFilePath);

            progress?.Report((modName, 100, "Complete"));
            return true;
        }
        catch (Exception ex)
        {
            progress?.Report((modResult.ServerMod.Name, 0, $"Error: {ex.Message}"));
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task BackupExistingFileAsync(string filePath, string modsFolder)
    {
        try
        {
            var backupDir = Path.Combine(modsFolder, "_backup");
            Directory.CreateDirectory(backupDir);

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var backupPath = Path.Combine(backupDir, $"{fileName}-{timestamp}.zip");

            File.Copy(filePath, backupPath, true);
        }
        catch
        {
            // Log error but don't fail the download
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _semaphore?.Dispose();
    }
}