using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using FSModLauncher.Models;
using Serilog;

namespace FSModLauncher.Services;

public class LocalModScanner(LocalModCacheService cacheService)
{
    public async Task<List<LocalMod>> ScanLocalModsAsync(string modsFolder, string hashAlgorithm)
    {
        var localMods = new List<LocalMod>();

        if (!Directory.Exists(modsFolder)) return localMods;

        var zipFiles = Directory.GetFiles(modsFolder, "*.zip", SearchOption.TopDirectoryOnly);

        await cacheService.LoadCacheAsync();
        await cacheService.CleanupCacheAsync(zipFiles);

        var processedCount = 0;
        var cachedCount = 0;

        foreach (var zipFile in zipFiles)
            try
            {
                var localMod = await ProcessZipFileAsync(zipFile, hashAlgorithm);
                if (localMod == null) continue;

                localMods.Add(localMod);
                processedCount++;

                // Check if this was from cache
                var fileInfo = new FileInfo(zipFile);
                var cacheEntry = cacheService.GetCacheEntry(zipFile);
                if (cacheEntry != null && LocalModCacheService.IsCacheEntryValid(cacheEntry, fileInfo))
                    cachedCount++;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to process mod file: {FilePath}", zipFile);
            }

        // Save cache after processing
        await cacheService.SaveCacheAsync();

        Log.Information("Processed {Total} mods ({Cached} from cache, {New} computed)",
            processedCount, cachedCount, processedCount - cachedCount);

        return localMods;
    }

    private async Task<LocalMod?> ProcessZipFileAsync(string zipFilePath, string hashAlgorithm)
    {
        var fileInfo = new FileInfo(zipFilePath);
        var modName = Path.GetFileNameWithoutExtension(zipFilePath);

        // Check cache first
        var cacheEntry = cacheService.GetCacheEntry(zipFilePath);
        string? hash = null;
        string? version = null;

        if (cacheEntry != null && LocalModCacheService.IsCacheEntryValid(cacheEntry, fileInfo))
        {
            // Use cached values
            hash = cacheEntry.Hash;
            version = cacheEntry.Version;
        }
        else
        {
            // Extract version from modDesc.xml
            version = await ExtractVersionFromZipAsync(zipFilePath);

            // Compute hash if required
            if (hashAlgorithm != "None")
                hash = await GiantsModHash.ComputeAsync(zipFilePath, modName);

            // Update cache
            await cacheService.UpdateCacheEntryAsync(
                zipFilePath,
                fileInfo.Length,
                fileInfo.LastWriteTimeUtc,
                hash,
                version);
        }

        var localMod = new LocalMod
        {
            Name = modName,
            FilePath = zipFilePath,
            SizeBytes = fileInfo.Length,
            Version = version,
            Hash = hash
        };

        return localMod;
    }

    private static async Task<string?> ExtractVersionFromZipAsync(string zipFilePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(zipFilePath);
            var modDescEntry = archive.Entries.FirstOrDefault(e =>
                e.Name.Equals("modDesc.xml", StringComparison.OrdinalIgnoreCase));

            if (modDescEntry == null)
                return null;

            await using var stream = modDescEntry.Open();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var doc = XDocument.Parse(content);
            return doc.Root?.Element("version")?.Value.Trim();
        }
        catch
        {
            return null;
        }
    }
}