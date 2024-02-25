using Kuti.Windows.VirtualDesktops;
using Windows.UI;
using System.Windows;
using Windows.UI.ViewManagement;
using WindowsDesktop;

namespace Kuti.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        var runtime = new Runtime();
        runtime.Register<IDesktopsManager>(() => new DesktopsManager());

        base.OnStartup(e);

        SetAppTheme();
        VirtualDesktop.Configure();
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

        settings.ColorValuesChanged += (sender, args) =>
        {
            var isLightTheme = IsColorLight(settings.GetColorValue(UIColorType.Background));
            // TODO Change app theme accordingly
        };

        // From https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-windows-themes?WT.mc_id=DT-MVP-5003978#know-when-dark-mode-is-enabled
        static bool IsColorLight(Color clr)
            => ((5 * clr.G) + (2 * clr.R) + clr.B) > (8 * 128);
    }
}

