using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WindowsDesktop;
using WindowsDesktop.Properties;

namespace Kuti.Windows.VirtualDesktops
{
    public interface IDesktopsManager
    {
        VirtualDesktop CurrentDesktop { get; }
        VirtualDesktop PreviousDesktop {  get; }

        event EventHandler<VirtualDesktopChangedEventArgs>? CurrentChanged;

        void Configure();

        VirtualDesktop? FindDesktop(string name, DesktopNameMatching matching = DesktopNameMatching.Exact);
    }

    public enum DesktopNameMatching
    {
        Exact = 0,
        StartsWith = 1,
    }

    internal class DesktopsManager : IDesktopsManager
    {
        public VirtualDesktop CurrentDesktop => VirtualDesktop.Current;

        public VirtualDesktop PreviousDesktop
        {
            get
            {
                if (_previousDesktop == null)
                {
                    string currentDesktopName = CurrentDesktop.Name;
                    _previousDesktop = VirtualDesktop.GetDesktops().FirstOrDefault(d => !currentDesktopName.Equals(d.Name)) ?? CurrentDesktop;
                }
                return _previousDesktop;
            }
        }

        public void Configure()
        {
            var assemblyDirPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "codoxide.com",
                Assembly.GetExecutingAssembly().GetName().Name ?? "Kuti",
                "assemblies"
            );
            VirtualDesktop.Configure(new () {
                CompiledAssemblySaveDirectory = new (assemblyDirPath)
            });
        }

        public VirtualDesktop? FindDesktop(string name, DesktopNameMatching matching = DesktopNameMatching.Exact)
        {
            var desktops = VirtualDesktop.GetDesktops();
            return matching == DesktopNameMatching.Exact ?
                desktops.FirstOrDefault(d => name.Equals(d.Name, StringComparison.CurrentCultureIgnoreCase)) :
                desktops.FirstOrDefault(d => d.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase));
        }

        public event EventHandler<VirtualDesktopChangedEventArgs>? CurrentChanged;

        private VirtualDesktop? _previousDesktop;

        public DesktopsManager()
        {
            string currentDesktop = VirtualDesktop.Current.Name;

            VirtualDesktop.CurrentChanged += (s, e) => {
                _previousDesktop = e.OldDesktop;
                if (CurrentChanged != null) CurrentChanged(s, e);
            };
        }
    }
}