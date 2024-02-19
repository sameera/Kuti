using Kuti.Windows.VirtualDesktops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WindowsDesktop;

namespace Kuti.Windows.QuickActions
{
    /// <summary>
    /// Interaction logic for QuicActionsWindow.xaml
    /// </summary>
    public partial class QuicActionsWindow : Window
    {
        private readonly IDesktopsRegistry _desktopsRegistry;
        private bool isFirstKeyPress = true;

        public QuicActionsWindow(IDesktopsRegistry? desktopsRegistry = null)
        {
            InitializeComponent();

            _desktopsRegistry = desktopsRegistry ?? new DesktopsRegistry();

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
                    ExecuteCommand();
                    break;

            }
        }

        private void ExecuteCommand()
        {
            var match = VirtualDesktop.GetDesktops().FirstOrDefault(d => d.Name.StartsWith(commandBox.Text, StringComparison.CurrentCultureIgnoreCase));
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
            commandBox.Focus();
            commandBox.Text = _desktopsRegistry.PreviousDesktop;
            commandBox.SelectAll();
        }
    }
}
