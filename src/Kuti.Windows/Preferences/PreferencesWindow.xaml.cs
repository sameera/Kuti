using Kuti.Windows.QuickActions;
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
    public partial class PreferencesWindow : Window
    {
        private readonly Dictionary<string, IPreferencesPage> _loadedPages = new Dictionary<string, IPreferencesPage>();

        public PreferencesWindow()
        {
            InitializeComponent();

            var shortcutSettingsPage = new HotkeysPage(Runtime.Current.GetInstance<IHotkeyManager>());
            _loadedPages.Add("shortcutSettings", shortcutSettingsPage);

            contentPane.Content = shortcutSettingsPage;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_loadedPages.Values.All(p => p.OnApply()))
            {
                UserSettings.Default.Save();
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
