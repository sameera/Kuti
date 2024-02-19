using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsDesktop;

namespace Kuti.Windows.VirtualDesktops
{
    public interface IDesktopsRegistry
    {
        string PreviousDesktop { get; }
    }
 
    internal class DesktopsRegistry: IDesktopsRegistry
    {
        public string PreviousDesktop { get; private set; }

        public DesktopsRegistry()
        {
            string currentDesktop = VirtualDesktop.Current.Name;
            PreviousDesktop = VirtualDesktop.GetDesktops()?.FirstOrDefault(d => d.Name != currentDesktop)?.Name ?? string.Empty;

            VirtualDesktop.CurrentChanged += (_, e) => PreviousDesktop = e.OldDesktop.Name;
        }
    }
}