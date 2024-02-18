using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kuti.Windows.Preferences
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly Dictionary<string, UserControl> _loadedPages = new Dictionary<string, UserControl>();

        public SettingsWindow()
        {
            InitializeComponent();

            var shortcutSettingsPage = new HotkeysPage();
            _loadedPages.Add("shortcutSettings", shortcutSettingsPage);

            contentPane.Content = shortcutSettingsPage;
        }
    }
}
