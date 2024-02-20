using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kuti.Windows.WindowsAPI
{
    internal static class User32
    {
        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        public const int SM_CXSIZEFRAME = 32;
        public const int SM_CXPADDEDBORDER = 92;
        public const int SM_CYSIZE = 30; // Approximation for the height of the title bar

        public const int WM_HOTKEY = 0x0312;
        public const uint MOD_NONE = 0x0000; // (none)
        public const uint MOD_ALT = 0x0001; // ALT
        public const uint MOD_CONTROL = 0x0002; // CTRL
        public const uint MOD_SHIFT = 0x0004; // SHIFT
        public const uint MOD_WIN = 0x0008; // WINDOWS
    }
}
