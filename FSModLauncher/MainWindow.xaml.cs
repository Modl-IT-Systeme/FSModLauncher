using System.Windows;
using FSModLauncher.Services;
using FSModLauncher.ViewModels;
using FSModLauncher.Views;

namespace FSModLauncher;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
    }

    public MainViewModel ViewModel => (MainViewModel)DataContext;

    private void InitializeServices()
    {
        var configService = new ConfigService();
        var cacheService = new LocalModCacheService();
        var serverModService = new ServerModService();
        var localModScanner = new LocalModScanner(cacheService);
        var comparerService = new ComparerService();
        var downloadService = new DownloadService();
        var gameLauncherService = new GameLauncherService();

        var mainViewModel = new MainViewModel(
            configService,
            serverModService,
            localModScanner,
            comparerService,
            downloadService,
            gameLauncherService);

        DataContext = mainViewModel;

        Loaded += async (s, e) => await mainViewModel.InitializeAsync();
    }

    private async void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var configService = new ConfigService();
        var settingsViewModel = new SettingsViewModel(configService);
        await settingsViewModel.LoadSettingsAsync();

        var settingsWindow = new SettingsWindow(settingsViewModel)
        {
            Owner = this
        };

        if (settingsWindow.ShowDialog() == true) await ViewModel.RefreshSettingsAsync();
    }
}