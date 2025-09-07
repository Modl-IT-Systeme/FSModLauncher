using System.Reflection;
using FSModLauncher.Models;
using Octokit;
using Serilog;

namespace FSModLauncher.Services;

public class UpdateService
{
    private readonly GitHubClient _gitHubClient;
    private readonly string _repositoryOwner = "Modl-IT-Systeme";
    private readonly string _repositoryName = "FSModLauncher";
    private DateTime _lastCheckTime = DateTime.MinValue;
    private GitHubRelease? _cachedLatestRelease;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromHours(1);

    public UpdateService()
    {
        _gitHubClient = new GitHubClient(new ProductHeaderValue("FSModLauncher"));
    }

    public async Task<GitHubRelease?> GetLatestReleaseAsync()
    {
        try
        {
            // Use cached result if it's still valid
            if (_cachedLatestRelease != null && DateTime.Now - _lastCheckTime < _cacheTimeout)
            {
                return _cachedLatestRelease;
            }

            Log.Information("Checking for updates from GitHub releases");
            
            var releases = await _gitHubClient.Repository.Release.GetAll(_repositoryOwner, _repositoryName);
            var latestRelease = releases
                .Where(r => !r.Draft && !r.Prerelease)
                .OrderByDescending(r => r.PublishedAt)
                .FirstOrDefault();

            if (latestRelease != null)
            {
                _cachedLatestRelease = new GitHubRelease
                {
                    TagName = latestRelease.TagName,
                    Name = latestRelease.Name ?? latestRelease.TagName,
                    Body = latestRelease.Body ?? "",
                    HtmlUrl = latestRelease.HtmlUrl,
                    PublishedAt = latestRelease.PublishedAt?.DateTime ?? DateTime.MinValue,
                    IsPrerelease = latestRelease.Prerelease,
                    IsDraft = latestRelease.Draft
                };
                
                _lastCheckTime = DateTime.Now;
                Log.Information("Latest release found: {Version}", _cachedLatestRelease.TagName);
                return _cachedLatestRelease;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to check for updates");
        }

        return null;
    }

    public async Task<bool> IsUpdateAvailableAsync()
    {
        var latestRelease = await GetLatestReleaseAsync();
        if (latestRelease == null) return false;

        var currentVersion = GetCurrentVersion();
        var latestVersion = ParseVersion(latestRelease.TagName);

        if (currentVersion == null || latestVersion == null) return false;

        return latestVersion > currentVersion;
    }

    public Version? GetCurrentVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to get current version");
            return null;
        }
    }

    private static Version? ParseVersion(string versionString)
    {
        try
        {
            // Remove 'v' prefix if present
            var cleanVersion = versionString.StartsWith('v') 
                ? versionString[1..] 
                : versionString;

            // Handle semantic versions like "1.0.0-beta" by taking only the version part
            var versionPart = cleanVersion.Split('-')[0];
            
            if (Version.TryParse(versionPart, out var version))
            {
                return version;
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to parse version: {Version}", versionString);
        }

        return null;
    }

    public void ClearCache()
    {
        _cachedLatestRelease = null;
        _lastCheckTime = DateTime.MinValue;
    }
}