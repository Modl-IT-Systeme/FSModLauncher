using System.IO;
using FSModLauncher.Models;
using Newtonsoft.Json;
using Serilog;

namespace FSModLauncher.Services;

public class LocalModCacheService
{
    private LocalModCache? _cache;

    public LocalModCacheService()
    {
    }

    public async Task<LocalModCache> LoadCacheAsync()
    {
        if (_cache != null)
            return _cache;

        try
        {
            if (!File.Exists(AppPaths.ModCacheFile))
            {
                _cache = new LocalModCache();
                await SaveCacheAsync();
                return _cache;
            }

            var json = await File.ReadAllTextAsync(AppPaths.ModCacheFile);
            _cache = JsonConvert.DeserializeObject<LocalModCache>(json) ?? new LocalModCache();

            // Validate cache version - if version mismatch, reset cache
            if (_cache.Version != 1)
            {
                Log.Information("Cache version mismatch, resetting cache");
                _cache = new LocalModCache();
                await SaveCacheAsync();
            }

            return _cache;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to load mod cache, creating new cache");
            _cache = new LocalModCache();
            await SaveCacheAsync();
            return _cache;
        }
    }

    public async Task SaveCacheAsync()
    {
        if (_cache == null)
            return;

        try
        {
            _cache.LastUpdated = DateTime.UtcNow;
            
            Directory.CreateDirectory(AppPaths.ConfigDirectory);

            var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
            await File.WriteAllTextAsync(AppPaths.ModCacheFile, json);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save mod cache");
        }
    }

    public LocalModCacheEntry? GetCacheEntry(string filePath)
    {
        if (_cache == null)
            return null;

        var fileName = Path.GetFileName(filePath);
        return _cache.Entries.GetValueOrDefault(fileName);
    }

    public static bool IsCacheEntryValid(LocalModCacheEntry cacheEntry, FileInfo fileInfo)
    {
        return cacheEntry.FileSize == fileInfo.Length &&
               Math.Abs((cacheEntry.LastModified - fileInfo.LastWriteTimeUtc).TotalSeconds) < 1;
    }

    public async Task UpdateCacheEntryAsync(string filePath, long fileSize, DateTime lastModified, string? hash, string? version)
    {
        _cache ??= await LoadCacheAsync();

        var fileName = Path.GetFileName(filePath);
        
        _cache.Entries[fileName] = new LocalModCacheEntry
        {
            FileName = fileName,
            FilePath = filePath,
            FileSize = fileSize,
            LastModified = lastModified,
            Hash = hash,
            Version = version
        };

        // Save cache periodically (not on every update for performance)
        // The LocalModScanner will call SaveCacheAsync when done scanning
    }

    public async Task CleanupCacheAsync(IEnumerable<string> existingFiles)
    {
        if (_cache == null)
            return;

        var existingFileNames = existingFiles.Select(Path.GetFileName).ToHashSet();
        var keysToRemove = _cache.Entries.Keys.Where(key => !existingFileNames.Contains(key)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Entries.Remove(key);
        }

        if (keysToRemove.Any())
        {
            Log.Information("Removed {Count} stale cache entries", keysToRemove.Count);
            await SaveCacheAsync();
        }
    }
}