using System.Runtime.InteropServices;

namespace Kuti.Windows.Common.WindowsAPI
{
    public static partial class User32
    {
        public const uint GW_CHILD = 5;
        public const uint WM_GETICON = 0x007F;
        public const uint ICON_SMALL = 0;
        public const uint ICON_BIG = 1;
        public const uint ICON_SMALL2 = 2;


        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);
    }
}
