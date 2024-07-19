using System.Runtime.InteropServices;

namespace Kuti.Windows.Common.WindowsAPI
{
    public static class Shell32
    {
        // Credit: https://stackoverflow.com/a/6008639/157552

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32")]
        public static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint flags);

        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        
        public const uint SHGFI_ICON = 0x000000100;     // get icon
        public const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
    }
}
