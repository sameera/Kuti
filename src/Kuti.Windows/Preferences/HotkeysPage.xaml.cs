using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kuti.Windows.Preferences;

/// <summary>
/// Interaction logic for HotkeysPage.xaml
/// </summary>
public partial class HotkeysPage : UserControl
{
    private bool _isWindowsKeyDown = false;

    public HotkeysPage()
    {
        InitializeComponent();

        menuHotKeyBox.PreviewKeyDown += MenuHotKeyBox_PreviewKeyDown;
        menuHotKeyBox.PreviewKeyUp += MenuHotKeyBox_PreviewKeyUp;
        menuHotKeyBox.KeyDown += MenuHotKeyBox_KeyDown;
    }

    private void MenuHotKeyBox_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LWin || e.Key == Key.RWin)
        {
            _isWindowsKeyDown = false;
        }
    }

    private void MenuHotKeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.LWin || e.Key == Key.RWin)
        {
            _isWindowsKeyDown = true;
        }
        // Prevent the text box from handling some key presses (e.g., space, backspace, etc.)
        else if (e.Key == Key.Space || e.Key == Key.Back || e.Key == Key.Delete ||
            e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down ||
            e.Key == Key.Home || e.Key == Key.End  ||
            e.Key == Key.LWin || e.Key == Key.RWin)
        {
            e.Handled = true;
        }
    }

    private void MenuHotKeyBox_KeyDown(object sender, KeyEventArgs e)
    {
        string modifiers = string.Empty;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            modifiers += "Ctrl+";
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            modifiers += "Shift+";
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            modifiers += "Alt+";
        if (_isWindowsKeyDown)
            modifiers += "Windows+";

        menuHotKeyBox.Text = modifiers + GetKeyName(e.Key);

        e.Handled = true;
    }

    private string GetKeyName(Key key) => new KeyConverter().ConvertToString(key) ?? string.Empty;
}