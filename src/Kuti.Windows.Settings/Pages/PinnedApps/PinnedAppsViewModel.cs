using Kuti.Windows.Common.VirtualDesktops;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using static Kuti.Windows.Common.WindowsAPI.Shell32;
using System.Windows.Input;
using Wpf.Ui.Input;

namespace Kuti.Windows.Settings.Pages.PinnedApps;

public partial class PinnedAppsViewModel
{
    private readonly IDesktopsManager _desktopsManager;

    public PinnableDesktop[] Desktops { get; private set; } = [];

    public ICommand PinningCommand { get; }

    public PinnedAppsViewModel(IDesktopsManager desktopsManager, IPinnedAppsRepository repository)
    {
        _desktopsManager = desktopsManager;

        PinningCommand = new RelayCommand<Tuple<PinnableProcess, PinnableDesktop>>(param => {
            if (param == null) return;

            var (process, desktop) = param;
            
            process.IsPinned = !process.IsPinned;
            repository.SavePin(process, desktop);
        });
    }

    public void RefreshModel()
    {
        var desktops = _desktopsManager.VirtualDesktops.ToDictionary(vd => vd.Id, vd => new PinnableDesktop(vd.Name, vd.Id));

        // Add a "virtual" virtual desktop for apps that are available on all desktops
        var allDesktop = new PinnableDesktop("(All Desktops)", Guid.Empty);
        desktops.Add(Guid.Empty, allDesktop);

        var processesByDesktop = desktops.Keys.ToDictionary(desktopId => desktopId, _ => new List<PinnableProcess>());

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
                var pinnable = new PinnableProcess(process.MainWindowTitle, exePath, GetAppIcon(exePath));
                pinnable.IsPinned = desktopId == Guid.Empty;

                processesByDesktop[desktopId].Add(pinnable);
            }
        }

        Desktops = processesByDesktop.Select(kv => {
            var desktop = desktops[kv.Key];
            desktop.Processes = kv.Value;
            return desktop;
        }).ToArray();
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
}
