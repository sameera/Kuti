using Kuti.Windows.Common;
using Kuti.Windows.Settings.Pages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;
using Wpf.Ui;

namespace Kuti.Windows.Settings;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static readonly IHost _host;

    static App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => _ = c.SetBasePath(AppContext.BaseDirectory))
            .ConfigureServices((services) => _ = services
                
                .AddCommonServices()
                .AddSingleton<INavigationService, NavigationService>()
                .AddSingleton<MainWindow>()

                // Settings Pages
                .AddSingleton<PinnedAppsPage>()
            )
            .Build();
    }

    public static T GetRequiredService<T>() where T : class => _host.Services.GetRequiredService<T>();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host.Start();

        var mainWindow = GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.StopAsync().Wait(); 
        _host.Dispose();

        base.OnExit(e);
    }
}

