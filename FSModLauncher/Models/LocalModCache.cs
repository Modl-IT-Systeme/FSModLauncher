namespace FSModLauncher.Models;

public class LocalModCacheEntry
{
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public string? Hash { get; set; }
    public string? Version { get; set; }
}

public class LocalModCache
{
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, LocalModCacheEntry> Entries { get; set; } = new();
}