using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSModLauncher.Models;
using FSModLauncher.Services;
using Microsoft.Win32;

namespace FSModLauncher.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ConfigService _configService;

    [ObservableProperty] private bool _backupBeforeOverwrite = true;

    [ObservableProperty] private string _cdnBaseUrl = "";

    [ObservableProperty] private int _concurrentDownloads = 3;

    [ObservableProperty] private string _gameExePath = "";

    [ObservableProperty] private string _hashAlgorithm = "MD5";

    [ObservableProperty] private string _modsFolder = "";

    [ObservableProperty] private string _serverIp = "";

    [ObservableProperty] private string _serverPort = "";

    [ObservableProperty] private string _serverCode = "";

    public SettingsViewModel(ConfigService configService)
    {
        _configService = configService;
    }

    public List<string> HashAlgorithmOptions { get; } = new() { "MD5", "SHA1", "None" };

    public async Task LoadSettingsAsync()
    {
        var settings = await _configService.LoadSettingsAsync();

        ModsFolder = settings.ModsFolder;
        GameExePath = settings.GameExePath;
        ServerIp = settings.ServerIp;
        ServerPort = settings.ServerPort;
        ServerCode = settings.ServerCode;
        HashAlgorithm = settings.HashAlgorithm;
        ConcurrentDownloads = settings.ConcurrentDownloads;
        BackupBeforeOverwrite = settings.BackupBeforeOverwrite;
    }

    [RelayCommand]
    private void BrowseModsFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Mods Folder"
        };

        if (!string.IsNullOrEmpty(ModsFolder) && Directory.Exists(ModsFolder)) dialog.InitialDirectory = ModsFolder;

        if (dialog.ShowDialog() == true) ModsFolder = dialog.FolderName;
    }

    [RelayCommand]
    private void BrowseGameExe()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Farming Simulator 25 Executable",
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (!string.IsNullOrEmpty(GameExePath))
        {
            var directory = Path.GetDirectoryName(GameExePath);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                dialog.InitialDirectory = directory;
                dialog.FileName = Path.GetFileName(GameExePath);
            }
        }

        if (dialog.ShowDialog() == true) GameExePath = dialog.FileName;
    }

    [RelayCommand]
    public async Task SaveSettings()
    {
        var settings = new AppSettings
        {
            ModsFolder = ModsFolder,
            GameExePath = GameExePath,
            ServerIp = ServerIp,
            ServerPort = ServerPort,
            ServerCode = ServerCode,
            HashAlgorithm = HashAlgorithm,
            ConcurrentDownloads = ConcurrentDownloads,
            BackupBeforeOverwrite = BackupBeforeOverwrite
        };

        await _configService.SaveSettingsAsync(settings);
    }

    public AppSettings GetCurrentSettings()
    {
        return new AppSettings
        {
            ModsFolder = ModsFolder,
            GameExePath = GameExePath,
            ServerIp = ServerIp,
            ServerPort = ServerPort,
            ServerCode = ServerCode,
            HashAlgorithm = HashAlgorithm,
            ConcurrentDownloads = ConcurrentDownloads,
            BackupBeforeOverwrite = BackupBeforeOverwrite
        };
    }
}