using Kuti.Windows.VirtualDesktops;
using Windows.UI;
using System.Windows;
using Windows.UI.ViewManagement;
using WindowsDesktop;
using Kuti.Windows.QuickActions;
using Serilog;
using System.IO;
using System.Reflection;
using Kuti.Windows.Preferences;
using Kuti.Windows.Common.VirtualDesktops;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Kuti.Windows.Common;

namespace Kuti.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IHost? _host;

    public static T GetRequiredService<T>() where T : class => _host!.Services.GetRequiredService<T>();

    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            Log.Fatal("Unhandled from {sender}: {@error}", e.ExceptionObject);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => _ = c.SetBasePath(AppContext.BaseDirectory))
            .ConfigureServices(services => {
                services
                    .AddCommonServices()
                    .AddSingleton<MainWindow>();
            })
            .Build();

        var appMeta = _host.Services.GetRequiredService<AppMetadata>();
        string basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            appMeta.Company,
            appMeta.ProductName
        );

        var runtime = new Runtime(_host.Services);
        runtime.Register(() => new MainWindow());
        runtime.Register<IHotkeyManager>(() => new HotkeyManager());

        string prefDbPath = Path.Combine(basePath, "preferences.db");
        runtime.Register<IPreferencesDb>(() => new PreferencesDb(prefDbPath));

        Log.Logger.Information("Starting up {version}", Assembly.GetExecutingAssembly().GetName().Version);
        base.OnStartup(e);

        Log.Logger.Debug("Loading the theme.");
        SetAppTheme();

        Log.Logger.Debug("Configuring the DesktopManager");
        runtime.GetInstance<IDesktopsManager>().Configure(appMeta);

        var mainWindow = runtime.GetInstance<MainWindow>();
        mainWindow.Loaded += (_, _) => runtime.GetInstance<IHotkeyManager>().Initialize(mainWindow);
        Log.Logger.Debug("Opening the MainWindow.");
        mainWindow.Show();
    }

    private void SetAppTheme()
    {
        var settings = new UISettings();
        var isLightTheme = IsColorLight(settings.GetColorValue(UIColorType.Background));

        if (isLightTheme)
        {
            ResourceDictionary lightThemeDict = new ResourceDictionary();
            lightThemeDict.Source = new Uri("Kuti.Windows;component/Preferences/Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute);
            Application.Current.Resources.MergedDictionaries.Add(lightThemeDict);
        }
        else 
        {
            ResourceDictionary darkThemeDict = new ResourceDictionary();
            darkThemeDict.Source = new Uri("Kuti.Windows;component/Preferences/Themes/DarkTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Add(darkThemeDict);
        }

        // From https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-windows-themes?WT.mc_id=DT-MVP-5003978#know-when-dark-mode-is-enabled
        static bool IsColorLight(Color clr)
            => ((5 * clr.G) + (2 * clr.R) + clr.B) > (8 * 128);
    }
}

