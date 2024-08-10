using Kuti.Windows.Common;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using WindowsDesktop;

using static Kuti.Windows.WindowsAPI.User32;

namespace Kuti.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool WasPinned = false;
    private readonly ILogger _logger;

    public MainWindow()
    {
        InitializeComponent();

        _logger = Runtime.Current.GetInstance<ILogger>();

        Loaded += MainWindow_Loaded;

        Unloaded += (_, _) => {
            VirtualDesktop.Switched -= VirtualDesktop_Switched;
        };

        Activated += (_, _) => {
            if (WasPinned) return;

            _logger.Debug("Pinning myself to all desktops.");

            try
            {
                this.Pin();
                Application.Current.Pin();
                WasPinned = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to pin.");
            }
        };
        
        MouseLeftButtonDown += (_, _) => DragMove();
        MouseLeftButtonUp += (_, _) => {
            if (Left == UserSettings.Default.MainWindowLocation.X && Top == UserSettings.Default.MainWindowLocation.Y) return; // The window was not moved.

            UserSettings.Default.MainWindowLocation = new System.Drawing.Point(((int)Left), (int)Top);
            UserSettings.Default.Save();
        };

        VirtualDesktop.Switched += VirtualDesktop_Switched;
    }

    private void VirtualDesktop_Switched(object? sender, VirtualDesktopSwitchedEventArgs e) => UpdateDesktopName(e.Desktop.Name);

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Following code was added to ensure that the window doesn't get lised in the Win+Tab list.
        // However, this causes the opacity and the rounded rectangle styles to be overridden and make
        // the window appear as a dark black rectangle. 
        var hwnd = new WindowInteropHelper(this).Handle;
        int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);

        UpdateDesktopName(VirtualDesktop.Current.Name);
        PositionWindow();
    }

    private void UpdateDesktopName(string name) => Dispatcher.Invoke(() => {
        CurrentDesktopName.Text = name;
        
        // We need reposition the window as width of the window may have changed.
        PositionWindow();
    });

    private void PositionWindow()
    {
        if (UserSettings.Default.MainWindowLocation.X > 0 && UserSettings.Default.MainWindowLocation.Y > 0)
        {
            Top = UserSettings.Default.MainWindowLocation.Y;
            Left = UserSettings.Default.MainWindowLocation.X;
        }
        else
        {
            int titleBarHeight = GetSystemMetrics(SM_CYSIZE);
            // Calculate the offset for the title bar buttons
            int offset = GetSystemMetrics(SM_CXSIZEFRAME) +
                         GetSystemMetrics(SM_CXPADDEDBORDER) +
                         titleBarHeight * 5; 
                        // Make room for titlebar buttons that might be behind this window (min, max, close) and add some buffer for any other buttons
                        // a 3rd party app might have added.

            // Get the screen width and height
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Set the window position
            Left = screenWidth - Width - offset;
            Top = 0 + (titleBarHeight - Height)/2;
        }

        _logger.Verbose(l => l.Verbose("Positioning the window at {Left}:{Top}", Left, Top));
    }

    private void MenuItemQuit_Click(object sender, RoutedEventArgs e) => Close();
    private void settingsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var procInfo = new ProcessStartInfo() {
            WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            FileName = string.Concat(Config.ExecutableName + ".Settings.exe")
        };

        if (procInfo.WorkingDirectory == null) throw new InvalidOperationException("Unable to resolve the Assembly location");

        string fullPath = Path.Combine(procInfo.WorkingDirectory, procInfo.FileName);

        if (!File.Exists(fullPath))
        {
            _logger.Warning(string.Concat(fullPath, " could not be found"));
            MessageBox.Show("We are unable to show the Settings configuration. "
                + "The requried component my not have been installed correctly. "
                + "Please try reinstalling the applicaiton.",
                "The Settings Editor was not found", MessageBoxButton.OK, MessageBoxImage.Warning
            );
            return;
        }

        try
        {
            // Start the app
            var settingsApp = Process.Start(procInfo);
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Settings application caused an error.");
        }
    }
}