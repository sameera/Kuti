using System.Windows;
using WindowsDesktop;

namespace Kuti.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        VirtualDesktop.Configure();
    }
}

