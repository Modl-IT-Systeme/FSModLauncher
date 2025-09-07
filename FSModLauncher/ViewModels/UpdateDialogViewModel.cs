using CommunityToolkit.Mvvm.ComponentModel;
using FSModLauncher.Models;

namespace FSModLauncher.ViewModels;

public partial class UpdateDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _currentVersion = "Unknown";

    [ObservableProperty] private string _latestVersion = "Unknown";

    [ObservableProperty] private string _releaseNotes = "";

    [ObservableProperty] private string _releaseDate = "";

    public string DownloadUrl { get; private set; } = "";

    public void Initialize(GitHubRelease latestRelease, Version? currentVersion)
    {
        CurrentVersion = currentVersion?.ToString() ?? "Unknown";
        LatestVersion = latestRelease.TagName;
        ReleaseNotes = !string.IsNullOrEmpty(latestRelease.Body) 
            ? latestRelease.Body 
            : "No release notes available.";
        ReleaseDate = $"Released on {latestRelease.PublishedAt:MMM d, yyyy}";
        DownloadUrl = latestRelease.HtmlUrl;
    }
}