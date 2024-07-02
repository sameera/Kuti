using Kuti.Windows.Common.VirtualDesktops;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static Kuti.Windows.Common.WindowsAPI.Shell32;

namespace Kuti.Windows.Settings.Pages
{
    public class PinnedAppsViewModel
    {
        private readonly IDesktopsManager _desktopsManager;

        public Desktop[] Desktops { get; private set; } = [];

        public PinnedAppsViewModel(IDesktopsManager desktopsManager)
        {
            _desktopsManager = desktopsManager;
        }

        public void RefreshModel()
        {
            var desktops = _desktopsManager.VirtualDesktops.ToDictionary(vd => vd.Id, vd => new Desktop(vd.Name, vd.Id));

            // Add a "virtual" virtual desktop for apps that are available on all desktops
            var allDesktop = new Desktop("(All Desktops)", Guid.Empty);
            desktops.Add(Guid.Empty, allDesktop);

            var foregroundApps = from p in Process.GetProcesses()
                                 where p.MainWindowHandle != 0 && !string.IsNullOrEmpty(p.MainWindowTitle) && !IsSystemProcess(p)
                                 select p;

            foreach (var process in foregroundApps.Distinct())
            {
                var desktop = _desktopsManager.FindDesktopFromWindow(process.MainWindowHandle);
                var desktopId = desktop?.Id ?? Guid.Empty;

                string? exePath = process.MainModule?.FileName;

                if (string.IsNullOrEmpty(exePath))
                {
                    // _logger.LogWarning(process.ProcessName + " does not have a valid path.");
                    continue;
                }

                if (desktops.TryGetValue(desktopId, out var targetDesktop))
                {
                    targetDesktop.Processes.Add(new PinnableProcess(process.MainWindowTitle, exePath, GetAppIcon(exePath)));
                }
            }

            Desktops = desktops.Values.ToArray();
        }

        private static ImageSource? GetAppIcon(string exePath)
        {
            // SHGFI_USEFILEATTRIBUTES takes the file name and attributes into account if it doesn't exist
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES | SHGFI_SMALLICON;

            uint attributes = FILE_ATTRIBUTE_NORMAL;

            if (0 != SHGetFileInfo(
                        exePath,
                        attributes,
                        out SHFILEINFO shfi,
                        (uint)Marshal.SizeOf(typeof(SHFILEINFO)),
                        flags))
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                            shfi.hIcon,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
            }
            return null;
        }

        private static bool IsSystemProcess(Process process)
        {
            try
            {
                return process.PriorityClass != ProcessPriorityClass.Normal
                    && process.MainModule?.FileName != null;
            }
            catch
            {
                return true;
            }
        }

        public record Desktop(string Name, Guid Id)
        {
            public List<PinnableProcess> Processes { get; } = [];

            public override string ToString() => Name;
        }

        public record PinnableProcess(string Name, string Path, ImageSource? Icon = null)
        {
            public override string ToString() => Name;
        }
    }
}
