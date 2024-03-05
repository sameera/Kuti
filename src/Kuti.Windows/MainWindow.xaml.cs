using Kuti.Windows.Preferences;
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

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;

        Unloaded += (_, _) => {
            VirtualDesktop.Switched -= VirtualDesktop_Switched;
        };

        Activated += (_, _) => {
            if (WasPinned) return;

            this.Pin();
            Application.Current.Pin();
            WasPinned = true;
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
            this.Top = UserSettings.Default.MainWindowLocation.Y;
            this.Left = UserSettings.Default.MainWindowLocation.X;
            return;
        }

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

    private void MenuItemQuit_Click(object sender, RoutedEventArgs e) => Close();
    private void settingsMenuItem_Click(object sender, RoutedEventArgs e) => new PreferencesWindow().ShowDialog();
}