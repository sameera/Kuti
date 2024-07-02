using Kuti.Windows.Common.VirtualDesktops;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WindowsDesktop;

namespace Kuti.Windows.VirtualDesktops
{
    /// <summary>
    /// Interaction logic for VirtualDesktopPickerWindow.xaml
    /// </summary>
    public partial class VirtualDesktopPickerWindow : Window, INotifyPropertyChanged
    {
        public IEnumerable<VirtualDesktop> AvailableDesktops { get; set; } = new VirtualDesktop[0];

        public bool SwitchToSelection { get; set; }

        private VirtualDesktop? _selectedDesktop;

        public event PropertyChangedEventHandler? PropertyChanged;

        public VirtualDesktop? SelectedDesktop
        {
            get { return _selectedDesktop; }
            set {
                _selectedDesktop = value; 
                OnPropertyChanged(nameof(SelectedDesktop));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public VirtualDesktopPickerWindow(IDesktopsManager? desktopManager = null)
        {
            InitializeComponent();
            DataContext = this;

            if (desktopManager != null)
            {
                AvailableDesktops = desktopManager.VirtualDesktops.Where(d => d != desktopManager.CurrentDesktop);
            }

            KeyUp += (_, e) => {
                if (e.Key == Key.Escape || e.Key == Key.Enter)
                {
                    SwitchToSelection = (e.Key == Key.Enter);
                    Close();
                }
            };

            Loaded += (_, _) => DesktopsListView.Focus();

            DesktopsListView.MouseUp += (_, _) => {
                SwitchToSelection = true;
                this.Close();
            };
        }
    }
}
