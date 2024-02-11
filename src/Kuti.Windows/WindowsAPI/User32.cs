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

        public const int SM_CXSIZEFRAME = 32;
        public const int SM_CXPADDEDBORDER = 92;
        public const int SM_CYSIZE = 30; // Approximation for the height of the title bar
    }
}
