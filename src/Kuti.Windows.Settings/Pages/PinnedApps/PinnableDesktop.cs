using CommunityToolkit.Mvvm.ComponentModel;

namespace Kuti.Windows.Settings.Pages.PinnedApps;

public partial class PinnableDesktop: ObservableObject
{
    [ObservableProperty]
    private List<PinnableProcess> _processes = [];

    public PinnableDesktop(string name, Guid id)
    {
        this.Name = name;
        this.Id = id;
    }

    public string Name { get; }
    public Guid Id { get; }

    public override string ToString() => Name;
}
