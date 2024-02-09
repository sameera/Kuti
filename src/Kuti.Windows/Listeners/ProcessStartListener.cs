using System.Management;

namespace Kuti.Windows.Listeners
{
    public class ProcessStartListener
    {
        public void StartListening()
        {
            var startWatch = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));

            startWatch.EventArrived += new EventArrivedEventHandler(OnProcessStarted);
            startWatch.Start();
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            string processName = e.NewEvent.GetPropertyValue("ProcessName") as string ?? "";
            string processId = e.NewEvent.GetPropertyValue("ProcessID") as string ?? "";
        }
    }
}
