using Kuti.Windows.WindowsAPI;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.Web.Syndication;

namespace Kuti.Windows.QuickActions;

public interface IHotkeyManager
{
    bool RegisterHotkeys();
    bool RegisterHotkeys(ModifierKeys modifiers, Key key);
    bool UnregisterHotkeys();

    (ModifierKeys modifiers, Key key) GetHotkeys();

    string GetKeyDisplayString(ModifierKeys modifierKeys, Key key);

    void Initialize(Window window);
}

internal class HotkeyManager : IHotkeyManager
{
    private const int HOTKEY_ID = 9000;

    private const int DEFAULT_MAIN_ACTION_KEY = 47; // 'D'
    private const int DEFAULT_MAIN_ACTION_MODIFIERS = 3; // Ctl+Alt

    private bool _isListening = false;

    private nint? _targetHwnd;

    public void Initialize(Window window)
    {            
        _targetHwnd = new WindowInteropHelper(window).Handle;
        RegisterHotkeys();

        window.Closing += (_, _) => {
            UnregisterHotkeys();
            _targetHwnd = null;
        };
    }

    public (ModifierKeys modifiers, Key key) GetHotkeys()
    {
        var settings = UserSettings.Default;
        int modifiers = settings.MainActionsHotkeyModifiers <= 0 ? DEFAULT_MAIN_ACTION_MODIFIERS : settings.MainActionsHotkeyModifiers;
        int key = settings.MainActionsHotkey <= 0 ? DEFAULT_MAIN_ACTION_KEY : settings.MainActionsHotkey;

        return ((ModifierKeys)modifiers, (Key)key);
    }


    public bool RegisterHotkeys()
    {
        var (modifiers, key) = GetHotkeys();
        return RegisterHotkeys(modifiers, key);
    }

    public bool UnregisterHotkeys() => User32.UnregisterHotKey(GetHandle(), HOTKEY_ID);

    public bool RegisterHotkeys(ModifierKeys modifiers, Key key)
    {
        UnregisterHotkeys();

        bool isRegisterd = User32.RegisterHotKey(
                GetHandle(), 
                HOTKEY_ID, 
                (uint)modifiers, 
                (uint)KeyInterop.VirtualKeyFromKey((Key)key));

        if (isRegisterd && !_isListening)
        {
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessage;
            _isListening = true;
        }
        else if (!isRegisterd)
        {
            Debug.WriteLine("Failed to register the Hot Keys.");
            Debug.Assert(false);
        }
        return isRegisterd;
    }

    public string GetKeyDisplayString(ModifierKeys modifierKeys, Key key)
    {
        StringBuilder keyString = new StringBuilder(20);

        if (modifierKeys.HasFlag(ModifierKeys.Control))
            keyString.Append("Ctrl+");
        if (modifierKeys.HasFlag(ModifierKeys.Shift))
            keyString.Append("Shift+");
        if (modifierKeys.HasFlag(ModifierKeys.Alt))
            keyString.Append("Alt+");
        if (modifierKeys.HasFlag(ModifierKeys.Windows))
            keyString.Append("Windows+");

        if (!IsModifierKey(key)) keyString.Append(GetKeyName(key));

        return keyString.ToString();
    }
    private string GetKeyName(Key key) => new KeyConverter().ConvertToString(key) ?? string.Empty;

    private bool IsModifierKey(Key key)
    {
        return key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LWin ||
                key == Key.RWin;
    }

    private void ThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message == User32.WM_HOTKEY && (int)msg.wParam == HOTKEY_ID)
        {
            new QuicActionsWindow().ShowDialog();
            handled = true;
        }
    }

    private nint GetHandle()
    {
        if (!_targetHwnd.HasValue)
        {
            throw new InvalidOperationException("The Hotkey Mananger must be initilized with a Target Window before invoking this method.");
        }
        return _targetHwnd!.Value;
    }
}