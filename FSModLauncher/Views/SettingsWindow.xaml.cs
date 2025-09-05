using System.Windows;
using FSModLauncher.ViewModels;

namespace FSModLauncher.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveSettings();
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}