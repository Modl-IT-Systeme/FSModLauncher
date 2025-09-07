using System.Windows;
using System.Windows.Controls;
using FSModLauncher.ViewModels;

namespace FSModLauncher.Views;

public partial class SettingsPage : UserControl
{
    public event EventHandler? SettingsSaved;

    public SettingsPage()
    {
        InitializeComponent();
    }

    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            await ViewModel.SaveSettings();
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}