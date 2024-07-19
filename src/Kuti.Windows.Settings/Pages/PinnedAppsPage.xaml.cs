using Kuti.Windows.Common.VirtualDesktops;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Kuti.Windows.Settings.Pages.PinnedAppsViewModel;
using System.Windows.Media;
using System.Diagnostics;
using Wpf.Ui.Controls;
using System.Linq;

namespace Kuti.Windows.Settings.Pages
{
    /// <summary>
    /// Interaction logic for PinnedAppsPage.xaml
    /// </summary>
    public partial class PinnedAppsPage : Page
    {
        private Point _startPoint;

        public PinnedAppsPage()
        {
            InitializeComponent();

            Loaded += (_, _) => {
                var viewModel = new PinnedAppsViewModel(App.GetRequiredService<IDesktopsManager>());
                viewModel.RefreshModel();
                DataContext = viewModel;
            };
        }

        private void ProcessList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(null);
                Vector diff = _startPoint - mousePos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    ListBox? listBox = sender as ListBox;
                    if (listBox == null) return;

                    PinnableDesktop? sourceDesktop = listBox.DataContext as PinnableDesktop;
                    if (sourceDesktop == null) return;

                    ListBoxItem? listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                    if (listBoxItem == null) return;

                    PinnableProcess process = (PinnableProcess)listBox.ItemContainerGenerator.ItemFromContainer(listBoxItem);
                    if (process == null) return;

                    DataObject dragData = new DataObject(nameof(DragData), new DragData(sourceDesktop, process));
                    DragDrop.DoDragDrop(listBoxItem, dragData, DragDropEffects.Move);
                }
            }
        }

        private void ProcessList_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(nameof(DragData)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ProcessList_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(nameof(DragData)) || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
            e.Handled = true;
        }

        private void ProcessList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(nameof(DragData)))
            {
                var dragData = e.Data.GetData(nameof(DragData)) as DragData;
                if (dragData == null) return;

                var dropTraget = sender as FrameworkElement;
                if (dropTraget == null) return;

                var targetDesktop = dropTraget.DataContext as PinnableDesktop;
                if (targetDesktop == null) return;

                var (sourceDesktop, process) = dragData;
                sourceDesktop.Processes = sourceDesktop.Processes.Where(p => p != process).ToArray();
                targetDesktop.Processes = targetDesktop.Processes.Append(process).ToArray();
            }
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private record DragData(PinnableDesktop SourceDesktop, PinnableProcess TargetProcess);
    }
}

