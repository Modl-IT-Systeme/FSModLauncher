namespace FSModLauncher.Models;

public class ComparisonResult
{
    public required ServerMod ServerMod { get; set; }
    public LocalMod? LocalMod { get; set; }
    public required ModStatus Status { get; set; }
}