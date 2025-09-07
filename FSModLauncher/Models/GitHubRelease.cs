namespace FSModLauncher.Models;

public class GitHubRelease
{
    public required string TagName { get; set; }
    public required string Name { get; set; }
    public required string Body { get; set; }
    public required string HtmlUrl { get; set; }
    public required DateTime PublishedAt { get; set; }
    public required bool IsPrerelease { get; set; }
    public required bool IsDraft { get; set; }
}