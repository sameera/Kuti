using Kuti.Windows.VirtualDesktops;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsDesktop;

using static Kuti.Windows.WindowsAPI.User32;

namespace Kuti.Windows.QuickActions
{
    /// <summary>
    /// Interaction logic for QuicActionsWindow.xaml
    /// </summary>
    public partial class QuicActionsWindow : Window
    {
        private bool isFirstKeyPress = true;
        private readonly IDesktopsManager _desktopManager;

        public QuicActionsWindow()
        {
            InitializeComponent();

            _desktopManager = Runtime.Current.GetInstance<IDesktopsManager>();

            Loaded += QuicActionsWindow_Loaded;
            KeyUp += QuicActionsWindow_KeyUp;

            PreviewKeyDown += QuicActionsWindow_PreviewKeyDown;
        }

        private void QuicActionsWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isFirstKeyPress && e.Key != Key.Enter) 
            { 
                isFirstKeyPress = false;
                commandBox.Text = string.Empty;
            }
        }

        private void QuicActionsWindow_KeyUp(object sender, KeyEventArgs e)
        {
          switch (e.Key) { 
                case Key.Escape:
                    Close();
                    break;
                case Key.Enter:
                    FocusDesktopByName(commandBox.Text);
                    break;
                case Key.Back:
                case Key.Delete:
                case Key.System:
                    break;
                default:
                    AutofillDesktopName();
                    break;

            }
        }

        private void AutofillDesktopName()
        {
            var match = _desktopManager.FindDesktop(commandBox.Text, DesktopNameMatching.StartsWith);
            if (match != null)
            {
                int selStart = commandBox.Text.Length;
                commandBox.Text = match.Name;
                commandBox.SelectionStart = selStart;
                commandBox.SelectionLength = commandBox.Text.Length - selStart;
            }
        }

        private void FocusDesktopByName(string desktopName)
        {
            var desktop = VirtualDesktop.GetDesktops().FirstOrDefault(d => desktopName.Equals(d.Name, StringComparison.CurrentCultureIgnoreCase));
            if (desktop == null) return;

            desktop.Switch();
            Close();
        }

        private void QuicActionsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetForegroundWindow(new WindowInteropHelper(this).Handle);
            CenterInMainMonitor();

            commandBox.Text = _desktopManager.PreviousDesktop.Name;
            commandBox.Focus();
            commandBox.SelectAll();
        }

        private void CenterInMainMonitor()
        {
            // Calculate the scale ratio
            double scaleRatioX = SystemParameters.VirtualScreenWidth / SystemParameters.PrimaryScreenWidth;
            double scaleRatioY = SystemParameters.VirtualScreenHeight / SystemParameters.PrimaryScreenHeight;

            // Calculate the window position to center it
            double centerX = (SystemParameters.VirtualScreenWidth - Width) / 2;
            double centerY = (SystemParameters.VirtualScreenHeight - Height) / 2;

            // Set window position
            Left = (SystemParameters.VirtualScreenLeft + centerX) / scaleRatioX;
            Top = (SystemParameters.VirtualScreenTop + centerY) / scaleRatioY;
        }
    }
}
