
using Kuti.Windows.WinAPI;
using WindowsDesktop;


namespace Kuti.Windows
{
    public partial class VirtualDesktopHelper : Form
    {
        private readonly ITaskbarAPI _taskbarAPI;

        protected override void OnLoad(EventArgs e)
        {
            label1.Text = VirtualDesktop.Current.Name;

            base.OnLoad(e);

            var taskbarRect = _taskbarAPI.GetTaskbarRect();
            Size = new Size(Size.Width, taskbarRect.Height / 2);
            Location = new Point(taskbarRect.Left + 10, taskbarRect.Top + taskbarRect.Height / 4);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            Close();
        }

        public VirtualDesktopHelper(ITaskbarAPI taskbarAPI)
        {
            InitializeComponent();

            _taskbarAPI = taskbarAPI;
            BackColor = taskbarAPI.GetTaskbarColor();
            ForeColor = taskbarAPI.GetTaskbarTextColor(BackColor);

            var taskbarRect = taskbarAPI.GetTaskbarRect();

            VirtualDesktop.Configure();
        }
    }
}
