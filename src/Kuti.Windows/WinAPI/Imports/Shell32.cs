using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kuti.Windows.WinAPI.Imports
{
    internal static class Shell32
    {
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        [DllImport("shell32.dll")]
        public static extern IntPtr SHAppBarMessage(int msg, ref APPBARDATA data);
    }
}
