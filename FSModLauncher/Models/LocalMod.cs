namespace FSModLauncher.Models;

public class LocalMod
{
    public required string Name { get; set; }
    public string? Version { get; set; }
    public string? Hash { get; set; }
    public required string FilePath { get; set; }
    public long SizeBytes { get; set; }
}