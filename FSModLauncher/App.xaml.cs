using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace FSModLauncher;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogException(e.ExceptionObject as Exception, "Unhandled exception");
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogException(e.Exception, "Dispatcher unhandled exception");
        e.Handled = true;
    }

    private static void LogException(Exception? exception, string context)
    {
        if (exception != null)
        {
            Log.Fatal(exception, "{Context}: {Message}", context, exception.Message);
            MessageBox.Show($"An error occurred: {exception.Message}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}