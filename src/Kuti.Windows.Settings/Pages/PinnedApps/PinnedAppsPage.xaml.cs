using Kuti.Windows.Common.VirtualDesktops;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace Kuti.Windows.Settings.Pages.PinnedApps;

/// <summary>
/// Interaction logic for PinnedAppsPage.xaml
/// </summary>
public partial class PinnedAppsPage : Page, INavigableView<PinnedAppsViewModel>
{
    private Point _startPoint;

    public PinnedAppsViewModel ViewModel { get; set; }

    public PinnedAppsPage()
    {
        InitializeComponent();
        ViewModel = new PinnedAppsViewModel(
            App.GetRequiredService<IDesktopsManager>(),
            App.GetRequiredService<IPinnedAppsRepository>()
        );

        Loaded += (_, _) => {
            ViewModel.RefreshModel();
            DataContext = ViewModel;
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
        if (!e.Data.GetDataPresent(nameof(DragData)))
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void ProcessList_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(nameof(DragData)))
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

            if (sourceDesktop.Id == targetDesktop.Id) return;

            sourceDesktop.Processes = sourceDesktop.Processes.Where(p => p != process).ToList();
            targetDesktop.Processes = targetDesktop.Processes.Append(process).Distinct().ToList();
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

