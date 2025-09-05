using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FSModLauncher.Models;

namespace FSModLauncher.ViewModels;

public partial class ModItemViewModel : ObservableObject
{
    [ObservableProperty] private ComparisonResult _comparisonResult;

    [ObservableProperty] private int _downloadProgress;

    [ObservableProperty] private string _downloadStatus = "";

    [ObservableProperty] private bool _isDownloading;

    public ModItemViewModel(ComparisonResult comparisonResult)
    {
        ComparisonResult = comparisonResult;
    }

    public string Name => ComparisonResult.ServerMod.Name;
    public string Title => ComparisonResult.ServerMod.Title;
    public string ServerVersion => ComparisonResult.ServerMod.Version;
    public string LocalVersion => ComparisonResult.LocalMod?.Version ?? "Not Installed";
    public ModStatus Status => ComparisonResult.Status;

    public string StatusText => Status switch
    {
        ModStatus.Latest => "Up to Date",
        ModStatus.Missing => "Missing",
        ModStatus.UpdateAvailable => "Update Available",
        _ => "Unknown"
    };

    public bool CanDownload => Status != ModStatus.Latest && !IsDownloading;
    public bool ShowDownloadButton => Status == ModStatus.Missing || Status == ModStatus.UpdateAvailable;

    partial void OnIsDownloadingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanDownload));
        DownloadCommand.NotifyCanExecuteChanged();
    }

    partial void OnComparisonResultChanged(ComparisonResult value)
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(ServerVersion));
        OnPropertyChanged(nameof(LocalVersion));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(CanDownload));
        OnPropertyChanged(nameof(ShowDownloadButton));
        DownloadCommand.NotifyCanExecuteChanged();
    }

    public event EventHandler<ModItemViewModel>? DownloadRequested;

    [RelayCommand(CanExecute = nameof(CanDownload))]
    private void Download()
    {
        DownloadRequested?.Invoke(this, this);
    }

    public void UpdateDownloadProgress(int progress, string status)
    {
        DownloadProgress = progress;
        DownloadStatus = status;
    }
}