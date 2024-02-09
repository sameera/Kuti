using System.ComponentModel;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static Kuti.Windows.WinAPI.Imports.Shell32;
using static Kuti.Windows.WinAPI.Imports.Gdi32;

namespace Kuti.Windows.WinAPI
{
    public interface ITaskbarAPI
    {
        Color GetTaskbarColor();
        Rectangle GetTaskbarRect();
        Color GetTaskbarTextColor(Color taskbarColor);
    }

    internal class TaskbarAPI : ITaskbarAPI
    {
        // Based on https://stackoverflow.com/questions/41805080/retrieve-color-of-windows-10-taskbar

        private const int ABM_GETTASKBARPOS = 5;

        public Rectangle GetTaskbarRect()
        {
            APPBARDATA data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf(data);

            IntPtr retval = SHAppBarMessage(ABM_GETTASKBARPOS, ref data);
            if (retval == IntPtr.Zero)
            {
                throw new Win32Exception("Please re-install Windows");
            }

            return new Rectangle(data.rc.left, data.rc.top, data.rc.right - data.rc.left, data.rc.bottom - data.rc.top);
        }

        private static Color GetColourAt(Point location)
        {
            using (Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, location.X, location.Y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }

                return screenPixel.GetPixel(0, 0);
            }
        }

        public Color GetTaskbarColor()
        {
            return GetColourAt(GetTaskbarRect().Location);
        }

        public Color GetTaskbarTextColor(Color taskbarColor)
        {
            // Determine brightness of the taskbar color
            double luminance = (0.299 * taskbarColor.R + 0.587 * taskbarColor.G + 0.114 * taskbarColor.B) / 255;

            // If luminance is high, use black text; otherwise, use white text
            return luminance > 0.5 ? Color.Black : Color.White;
        }
    }
}
