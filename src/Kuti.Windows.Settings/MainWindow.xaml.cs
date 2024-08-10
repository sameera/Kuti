using Kuti.Windows.Settings.Pages.PinnedApps;
using System.Windows;
using WinRT;
using Wpf.Ui.Controls;

namespace Kuti.Windows.Settings;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(/*INavigationService navigationService, IServiceProvider serviceProvider*/)
    {
        InitializeComponent();


        Loaded += (_, _) => {
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(
                this,
                Wpf.Ui.Controls.WindowBackdropType.Mica,
                true
            );
            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Dark);
            
            _navigationView.Navigate(typeof(PinnedAppsPage));
        };
    }
}