using System.Diagnostics;
using System.Windows;
using FSModLauncher.ViewModels;

namespace FSModLauncher.Views;

public partial class UpdateDialog : Window
{
    public UpdateDialog()
    {
        InitializeComponent();
    }

    public UpdateDialogViewModel ViewModel => (UpdateDialogViewModel)DataContext;

    private void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(ViewModel.DownloadUrl))
            {
                // Open the GitHub releases page in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = ViewModel.DownloadUrl,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to open download page: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        DialogResult = true;
        Close();
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}