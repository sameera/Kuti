using Kuti.Windows.VirtualDesktops;
using System.Windows.Controls;
using System.Diagnostics;
using Serilog;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

using static Kuti.Windows.WindowsAPI.Shell32;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Text.Json.Nodes;
using System.Text.Json;
using Kuti.Windows.Common.VirtualDesktops;

namespace Kuti.Windows.Preferences.Themes
{
    /// <summary>
    /// Interaction logic for AppToDesktopMappingsPage.xaml
    /// </summary>
    public partial class AppToDesktopMappingsPage : UserControl, IPreferencesPage
    {
        private readonly IDesktopsManager _desktopsManager;
        private readonly ILogger _logger;
        private readonly IPreferencesDb _preferences;

        private readonly Dictionary<Guid, HashSet<ProcessItem>> _processByDesktopId = new Dictionary<Guid, HashSet<ProcessItem>>();
        private readonly Dictionary<string, HashSet<ProcessItem>> _pinnedAppsByDesktopName = new Dictionary<string, HashSet<ProcessItem>>();

        private bool _wasInitialized = false;

        private ProcessItem? _draggedItem = null;
        private ListBox? _sourceList = null;

        public AppToDesktopMappingsPage(IPreferencesDb preferences, IDesktopsManager desktopsManager, ILogger logger)
        {
            InitializeComponent();

            appsList.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            appsList.PreviewMouseMove += ListBox_PreviewMouseMove;
            appsList.PreviewMouseLeftButtonUp += ListBox_PreviewMouseLeftButtonUp;
            appsList.Drop += ListBox_Drop;
            appsList.DragEnter += ListBox_DragEnter;
            appsList.DragOver += ListBox_DragOver;

            pinnedList.PreviewMouseLeftButtonDown += ListBox_PreviewMouseLeftButtonDown;
            pinnedList.PreviewMouseMove += ListBox_PreviewMouseMove;
            pinnedList.PreviewMouseLeftButtonUp += ListBox_PreviewMouseLeftButtonUp;
            pinnedList.Drop += ListBox_Drop;
            pinnedList.DragEnter += ListBox_DragEnter;
            pinnedList.DragOver += ListBox_DragOver;

            _preferences = preferences;
            _desktopsManager = desktopsManager;
            _logger = logger;

            desktopList.SelectionChanged += DesktopList_SelectionChanged;
        }

        public bool OnApply()
        {
            foreach (var kv in _pinnedAppsByDesktopName)
            {
                var processes = from p in kv.Value
                                orderby p.Name
                                select new { p.Name, p.Path };

                _preferences.SetPreference(kv.Key, JsonSerializer.Serialize(processes.ToArray()));
            }
            return true;
        }

        public void OnShow()
        {
            if (!_wasInitialized) RefreshView();
        }

        private void RefreshView()
        {
            desktopList.Items.Clear();
            _processByDesktopId.Clear();

            _pinnedAppsByDesktopName.Clear();

            foreach (var desktop in _desktopsManager.VirtualDesktops)
            {
                desktopList.Items.Add(new DesktopItem(desktop.Name, desktop.Id));
                _processByDesktopId.Add(desktop.Id, new HashSet<ProcessItem>(3));
                _pinnedAppsByDesktopName.Add(desktop.Name, new HashSet<ProcessItem>());
            }

            desktopList.Items.Add(new DesktopItem("(All Desktops)", Guid.Empty));
            _processByDesktopId.Add(Guid.Empty, new HashSet<ProcessItem>(3));

            PopulateRunningProcesses();
        }

        private void PopulateRunningProcesses()
        {
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
                    _logger.Warning(process.ProcessName + " does not have a valid path.");
                    continue;
                }

                _processByDesktopId[desktopId].Add(new(
                    process.MainWindowTitle,
                    exePath,
                    GetAppIcon(exePath)
                ));
            }

            _wasInitialized = true;
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

        private void DesktopList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            appsList.Items.Clear();

            var selectedDesktop = desktopList.SelectedItem as DesktopItem;
            if (selectedDesktop == null) return;

            if (_processByDesktopId.TryGetValue(selectedDesktop.Id, out var processes))
            {
                foreach ( var process in processes)
                {
                    appsList.Items.Add(process);
                }
            }

            pinnedList.Items.Clear();

            string desktopName = selectedDesktop.Name;
            string pinnedAppsJson = _preferences.GetPreference("Pinned_apps_to_" + desktopName) ?? "[]";

            // TODO: Unsafe deserialization. Fix this.
            var pinnedApps = JsonSerializer.Deserialize<ProcessItem[]>(pinnedAppsJson) ?? [];

            for ( var i = 0; i < pinnedApps.Length; i++)
            {
                _pinnedAppsByDesktopName[desktopName].Add(pinnedApps[i]);
            }

            foreach ( var process in _processByDesktopId[selectedDesktop.Id])
            {
                pinnedList.Items.Add(process);
            }
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

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListBox)) return;

            ListBox listBox = (ListBox)sender;
            _draggedItem = GetObjectDataFromPoint(listBox, e.GetPosition(listBox));
            if (_draggedItem != null)
            {
                _sourceList = listBox;
            }
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_sourceList != null && e.LeftButton == MouseButtonState.Pressed && sender is ListBox)
            {
                ListBox listBox = (ListBox)sender;
                Point position = e.GetPosition(listBox);

                if (Math.Abs(position.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(listBox, _draggedItem, DragDropEffects.Move);
                    _sourceList = null;
                }
            }
        }

        private void ListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _sourceList = null;
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ProcessItem)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(ProcessItem)))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ProcessItem)) && sender is ListBox)
            {
                ProcessItem droppedData = (ProcessItem)e.Data.GetData(typeof(ProcessItem));
                ListBox listBox = (ListBox)sender;
                listBox.Items.Add(droppedData);

                _sourceList?.Items.Remove(droppedData);
                _sourceList = null;

                var desktop = desktopList.SelectedItem as DesktopItem ?? throw new InvalidOperationException("Pinning while no desktop is selected.");
                _pinnedAppsByDesktopName[desktop.Name].Add(droppedData);
            }
        }

        private ProcessItem? GetObjectDataFromPoint(ListBox listBox, Point point)
        {
            // Find the ListBoxItem at the specified point
            DependencyObject obj = (DependencyObject)listBox.InputHitTest(point);
            while (obj != null && obj != listBox)
            {
                if (obj is ListBoxItem)
                {
                    // Return the data object associated with the ListBoxItem
                    return ((ListBoxItem)obj).DataContext as ProcessItem;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        private record ProcessItem(string Name, string Path, ImageSource? Icon = null)
        {
            public override string ToString() => Name;
        }

        private record DesktopItem(string Name, Guid Id)
        {
            public override string ToString() => Name;
        }

        [GeneratedRegex(@"^(?!.*\\Windows\\).*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        private static partial Regex GenerateSystemPathPattern();
    }
}
