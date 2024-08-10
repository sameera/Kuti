using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace Kuti.Windows.Settings.Pages.PinnedApps;

public partial class PinnableProcess(string name, string path, ImageSource? icon = null) : ObservableObject
{
    [ObservableProperty]
    private bool _isPinned;

    public string Name { get; } = name;
    public string Path { get; } = path;
    public ImageSource? Icon { get; } = icon;

    public override string ToString() => Name;
}
