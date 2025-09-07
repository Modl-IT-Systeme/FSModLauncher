using System.Windows;
using System.Windows.Controls;
using ModernWpf.Controls;
using FSModLauncher.Services;
using FSModLauncher.ViewModels;
using FSModLauncher.Views;

namespace FSModLauncher;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private SettingsPage? _settingsPage;
    private SettingsViewModel? _settingsViewModel;

    public MainWindow()
    {
        InitializeComponent();
        InitializeServices();
        
        // Set the default selection for NavigationView and show Mod Manager page
        NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems[0];
        ShowModManagerPage();
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
        var updateService = new UpdateService();

        var mainViewModel = new MainViewModel(
            configService,
            serverModService,
            localModScanner,
            comparerService,
            downloadService,
            gameLauncherService,
            updateService);

        DataContext = mainViewModel;

        Loaded += async (s, e) => await mainViewModel.InitializeAsync();
    }


    // Custom window chrome event handlers
    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click to maximize/restore
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        else
        {
            // Single click to drag
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            switch (tag)
            {
                case "ModManager":
                    ShowModManagerPage();
                    break;
                case "Settings":
                    ShowSettingsPage();
                    break;
            }
        }
    }

    private void ShowModManagerPage()
    {
        ContentPresenter.Content = null;
        ModManagerPage.Visibility = Visibility.Visible;
    }

    private async void ShowSettingsPage()
    {
        ModManagerPage.Visibility = Visibility.Collapsed;
        
        if (_settingsPage == null)
        {
            var configService = new ConfigService();
            _settingsViewModel = new SettingsViewModel(configService);
            await _settingsViewModel.LoadSettingsAsync();
            
            _settingsPage = new SettingsPage();
            _settingsPage.DataContext = _settingsViewModel;
            _settingsPage.SettingsSaved += async (s, e) => await ViewModel.RefreshSettingsAsync();
        }
        
        ContentPresenter.Content = _settingsPage;
    }
}