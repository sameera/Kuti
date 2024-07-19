using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Kuti.Windows.Settings.Pages
{
    public record PinnableDesktop(string Name, Guid Id): INotifyPropertyChanged
    {
        private IReadOnlyList<PinnableProcess> _processes = [];

        public IReadOnlyList<PinnableProcess> Processes 
        { 
            get { return _processes;  }
            set {
                _processes = value;
                OnPropertyChanged(nameof(Processes));
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
