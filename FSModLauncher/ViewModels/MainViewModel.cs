using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSModLauncher.Models;
using FSModLauncher.Services;
using FSModLauncher.Views;
using Serilog;

namespace FSModLauncher.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ComparerService _comparerService;
    private readonly ConfigService _configService;
    private readonly DownloadService _downloadService;
    private readonly GameLauncherService _gameLauncherService;
    private readonly LocalModScanner _localModScanner;
    private readonly ServerModService _serverModService;
    private readonly UpdateService _updateService;

    private AppSettings? _currentSettings;

    [ObservableProperty] private bool _isDownloading;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private ObservableCollection<ModItemViewModel> _mods = new();

    [ObservableProperty] private int _overallProgress;

    [ObservableProperty] private string _serverStatus = "Not Connected";

    [ObservableProperty] private string _statusMessage = "Ready";

    [ObservableProperty] private bool _updateAvailable;

    [ObservableProperty] private string _updateVersion = "";

    [ObservableProperty] private bool _checkingForUpdates;

    public MainViewModel(
        ConfigService configService,
        ServerModService serverModService,
        LocalModScanner localModScanner,
        ComparerService comparerService,
        DownloadService downloadService,
        GameLauncherService gameLauncherService,
        UpdateService updateService)
    {
        _configService = configService;
        _serverModService = serverModService;
        _localModScanner = localModScanner;
        _comparerService = comparerService;
        _downloadService = downloadService;
        _gameLauncherService = gameLauncherService;
        _updateService = updateService;
    }

    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();

        // Initialize logging
        var logsPath = Path.Combine(_configService.GetLogsPath(), "launcher.log");
        Directory.CreateDirectory(_configService.GetLogsPath());

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(logsPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        Log.Information("Application started");

        // Automatically check mods on startup
        await CheckMods();
        
        // Check for updates in the background
        _ = Task.Run(async () => await CheckForUpdatesAsync());
    }

    private async Task LoadSettingsAsync()
    {
        _currentSettings = await _configService.LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task CheckMods()
    {
        if (_currentSettings == null || IsLoading)
            return;

        IsLoading = true;
        StatusMessage = "Checking mods...";

        try
        {
            // Fetch server mods
            StatusMessage = "Fetching server mod list...";
            var serverMods = await _serverModService.FetchServerModsAsync(_currentSettings.GetServerStatsUrl());

            ServerStatus = $"Connected - {serverMods.Count} server mods";
            Log.Information("Found {Count} server mods", serverMods.Count);

            // Scan local mods
            StatusMessage = "Scanning local mods...";
            var localMods = await _localModScanner.ScanLocalModsAsync(
                _currentSettings.ModsFolder,
                _currentSettings.HashAlgorithm);

            Log.Information("Found {Count} local mods", localMods.Count);

            // Compare mods
            StatusMessage = "Comparing mods...";
            var comparisonResults = _comparerService.CompareModsAsync(
                serverMods,
                localMods,
                _currentSettings.HashAlgorithm);

            // Update UI - sort by priority: Missing, UpdateAvailable, Latest
            Mods.Clear();
            var sortedResults = comparisonResults.OrderBy(r => r.Status);
            foreach (var result in sortedResults)
            {
                var modVm = new ModItemViewModel(result);
                modVm.DownloadRequested += OnModDownloadRequested;
                Mods.Add(modVm);
            }

            var missingCount = comparisonResults.Count(r => r.Status == ModStatus.Missing);
            var updateCount = comparisonResults.Count(r => r.Status == ModStatus.UpdateAvailable);
            var upToDateCount = comparisonResults.Count(r => r.Status == ModStatus.Latest);

            StatusMessage =
                $"Complete - {missingCount} missing, {updateCount} updates available, {upToDateCount} up to date";
            Log.Information("Mod check complete: {Missing} missing, {Updates} updates, {UpToDate} up to date",
                missingCount, updateCount, upToDateCount);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            ServerStatus = "Connection Failed";
            Log.Error(ex, "Failed to check mods");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadMissingAndUpdates()
    {
        if (_currentSettings == null || IsDownloading || IsLoading)
            return;

        var modsToDownload = Mods
            .Where(m => m.Status == ModStatus.Missing || m.Status == ModStatus.UpdateAvailable)
            .ToList();

        if (!modsToDownload.Any())
        {
            StatusMessage = "No mods need downloading";
            return;
        }

        IsDownloading = true;
        OverallProgress = 0;

        try
        {
            var completedDownloads = 0;
            var totalDownloads = modsToDownload.Count;

            var downloadTasks = modsToDownload.Select(async modVm =>
            {
                modVm.IsDownloading = true;

                var progress = new Progress<(string ModName, int ProgressPercentage, string Status)>(p =>
                {
                    modVm.UpdateDownloadProgress(p.ProgressPercentage, p.Status);
                });

                var success = await _downloadService.DownloadModAsync(
                    modVm.ComparisonResult,
                    _currentSettings.GetCdnBaseUrl(),
                    _currentSettings.ModsFolder,
                    _currentSettings.HashAlgorithm,
                    _currentSettings.BackupBeforeOverwrite,
                    progress);

                modVm.IsDownloading = false;

                if (success)
                {
                    // Update status to Latest after successful download
                    var updatedResult = new ComparisonResult
                    {
                        ServerMod = modVm.ComparisonResult.ServerMod,
                        LocalMod = new LocalMod
                        {
                            Name = modVm.ComparisonResult.ServerMod.Name,
                            Version = modVm.ComparisonResult.ServerMod.Version,
                            Hash = modVm.ComparisonResult.ServerMod.Hash,
                            FilePath = Path.Combine(_currentSettings.ModsFolder,
                                $"{modVm.ComparisonResult.ServerMod.Name}.zip"),
                            SizeBytes = 0
                        },
                        Status = ModStatus.Latest
                    };
                    modVm.ComparisonResult = updatedResult;
                    Log.Information("Successfully downloaded {ModName}", modVm.Name);
                }
                else
                {
                    Log.Error("Failed to download {ModName}", modVm.Name);
                }

                Interlocked.Increment(ref completedDownloads);
                OverallProgress = completedDownloads * 100 / totalDownloads;

                return success;
            });

            StatusMessage = $"Downloading {totalDownloads} mods...";
            var results = await Task.WhenAll(downloadTasks);

            var successCount = results.Count(r => r);
            var failedCount = totalDownloads - successCount;

            StatusMessage = failedCount == 0
                ? $"All {successCount} mods downloaded successfully"
                : $"Downloaded {successCount} mods, {failedCount} failed";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download error: {ex.Message}";
            Log.Error(ex, "Failed to download mods");
        }
        finally
        {
            IsDownloading = false;
            OverallProgress = 0;
        }
    }

    [RelayCommand]
    private async Task LaunchGame()
    {
        if (_currentSettings == null)
            return;

        try
        {
            StatusMessage = "Launching Farming Simulator 25...";
            var success = await _gameLauncherService.LaunchGameAsync(_currentSettings.GameExePath);

            if (success)
            {
                StatusMessage = "Game launched successfully";
                Log.Information("Game launched successfully");
            }
            else
            {
                StatusMessage = "Failed to launch game";
                Log.Error("Failed to launch game");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Launch error: {ex.Message}";
            Log.Error(ex, "Failed to launch game");
        }
    }

    [RelayCommand]
    private void OpenModsFolder()
    {
        if (_currentSettings == null || string.IsNullOrEmpty(_currentSettings.ModsFolder))
            return;

        try
        {
            if (Directory.Exists(_currentSettings.ModsFolder))
                Process.Start("explorer.exe", _currentSettings.ModsFolder);
            else
                StatusMessage = "Mods folder does not exist";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to open folder: {ex.Message}";
            Log.Error(ex, "Failed to open mods folder");
        }
    }

    public async Task RefreshSettingsAsync()
    {
        await LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        await CheckForUpdatesAsync();
    }

    [RelayCommand]
    private async Task ShowUpdateDialog()
    {
        var latestRelease = await _updateService.GetLatestReleaseAsync();
        if (latestRelease == null) return;

        var currentVersion = _updateService.GetCurrentVersion();
        
        var updateDialog = new UpdateDialog();
        var updateViewModel = new UpdateDialogViewModel();
        updateViewModel.Initialize(latestRelease, currentVersion);
        updateDialog.DataContext = updateViewModel;
        
        // Show dialog - we'll need to get the parent window
        updateDialog.Owner = System.Windows.Application.Current.MainWindow;
        updateDialog.ShowDialog();
    }

    private async Task CheckForUpdatesAsync()
    {
        if (CheckingForUpdates) return;

        CheckingForUpdates = true;
        try
        {
            Log.Information("Checking for application updates");
            var isUpdateAvailable = await _updateService.IsUpdateAvailableAsync();
            
            UpdateAvailable = isUpdateAvailable;
            if (isUpdateAvailable)
            {
                var latestRelease = await _updateService.GetLatestReleaseAsync();
                UpdateVersion = latestRelease?.TagName ?? "Unknown";
                Log.Information("Update available: {Version}", UpdateVersion);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to check for updates");
            UpdateAvailable = false;
        }
        finally
        {
            CheckingForUpdates = false;
        }
    }

    private async void OnModDownloadRequested(object? sender, ModItemViewModel modViewModel)
    {
        if (_currentSettings == null || modViewModel.IsDownloading)
            return;

        await DownloadSingleMod(modViewModel);
    }

    private async Task DownloadSingleMod(ModItemViewModel modViewModel)
    {
        if (_currentSettings == null)
            return;

        modViewModel.IsDownloading = true;

        try
        {
            var progress = new Progress<(string ModName, int ProgressPercentage, string Status)>(p =>
            {
                modViewModel.UpdateDownloadProgress(p.ProgressPercentage, p.Status);
            });

            var success = await _downloadService.DownloadModAsync(
                modViewModel.ComparisonResult,
                _currentSettings.GetCdnBaseUrl(),
                _currentSettings.ModsFolder,
                _currentSettings.HashAlgorithm,
                _currentSettings.BackupBeforeOverwrite,
                progress);

            if (success)
            {
                // Update status to Latest after successful download
                var updatedResult = new ComparisonResult
                {
                    ServerMod = modViewModel.ComparisonResult.ServerMod,
                    LocalMod = new LocalMod
                    {
                        Name = modViewModel.ComparisonResult.ServerMod.Name,
                        Version = modViewModel.ComparisonResult.ServerMod.Version,
                        Hash = modViewModel.ComparisonResult.ServerMod.Hash,
                        FilePath = Path.Combine(_currentSettings.ModsFolder,
                            $"{modViewModel.ComparisonResult.ServerMod.Name}.zip"),
                        SizeBytes = 0
                    },
                    Status = ModStatus.Latest
                };
                modViewModel.ComparisonResult = updatedResult;
                Log.Information("Successfully downloaded {ModName}", modViewModel.Name);

                StatusMessage = $"Successfully downloaded {modViewModel.Name}";
            }
            else
            {
                Log.Error("Failed to download {ModName}", modViewModel.Name);
                StatusMessage = $"Failed to download {modViewModel.Name}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download error for {modViewModel.Name}: {ex.Message}";
            Log.Error(ex, "Failed to download {ModName}: {Message}", modViewModel.Name, ex.Message);
        }
        finally
        {
            modViewModel.IsDownloading = false;
        }
    }
}