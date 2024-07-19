using System.Runtime.InteropServices;

namespace Kuti.Windows.Common.WindowsAPI
{
    public static partial class User32
    {
        public const int WM_HOTKEY = 0x0312;
        public const uint MOD_NONE = 0x0000; // (none)
        public const uint MOD_ALT = 0x0001; // ALT
        public const uint MOD_CONTROL = 0x0002; // CTRL
        public const uint MOD_SHIFT = 0x0004; // SHIFT
        public const uint MOD_WIN = 0x0008; // WINDOWS

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
