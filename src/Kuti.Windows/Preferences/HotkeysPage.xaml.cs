using Kuti.Windows.QuickActions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kuti.Windows.Preferences;

/// <summary>
/// Interaction logic for HotkeysPage.xaml
/// </summary>
public partial class HotkeysPage : UserControl, IPreferencesPage
{

    private bool _isWindowsKeyDown = false;
    private ModifierKeys _selectedModifiers = ModifierKeys.None;
    private Key _selectedKey = Key.None;

    private readonly IHotkeyManager _hotkeyManager;

    public string Title { get; } = "Hotkey Settings";

    public HotkeysPage(IHotkeyManager hotKeyManager)
    {
        InitializeComponent();

        _hotkeyManager = hotKeyManager;
        (_selectedModifiers, _selectedKey) = hotKeyManager.GetHotkeys();

        menuHotKeyBox.PreviewKeyDown += MenuHotKeyBox_PreviewKeyDown;
        menuHotKeyBox.PreviewKeyUp += MenuHotKeyBox_PreviewKeyUp;
        menuHotKeyBox.KeyDown += MenuHotKeyBox_KeyDown;

        menuHotKeyBox.Loaded += (_, _) => {
            menuHotKeyBox.Text = hotKeyManager.GetKeyDisplayString(_selectedModifiers, _selectedKey);
        };
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
        _selectedModifiers = Keyboard.Modifiers;
        _selectedKey = e.Key;

        if (_isWindowsKeyDown)
        {
            _selectedModifiers |= ModifierKeys.Windows;
        }

        menuHotKeyBox.Text = _hotkeyManager.GetKeyDisplayString(
            _selectedModifiers, 
            _selectedKey);

        e.Handled = true;
    }

    public bool OnApply()
    {
        var keyConverter = new KeyConverter();

        if (_isWindowsKeyDown)
        {
            _selectedModifiers |= ModifierKeys.Windows;
        }

        if (_hotkeyManager.RegisterHotkeys(_selectedModifiers, _selectedKey))
        {
            var settings = UserSettings.Default;
            settings.MainActionsHotkey = (int)_selectedKey;
            settings.MainActionsHotkeyModifiers = (int)_selectedModifiers;
            return true;
        }
        else
        {
            MessageBox.Show(
                "Hotkey Assignment Failed", "Unable to register the specified hot key.\n" +
                "It may already be registered by another application.", 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
            return false;
        }
    }
}