using Kuti.Windows.Settings.Pages;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui;

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