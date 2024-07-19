using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Kuti.Windows.Settings.Pages
{
    public record PinnableProcess(string Name, string Path, ImageSource? Icon = null) : INotifyPropertyChanged
    {
        private bool _isPinned;

        public bool IsPinned
        {
            get => _isPinned;
            set {
                if (_isPinned != value)
                {
                    _isPinned = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() => Name;
    }
}
