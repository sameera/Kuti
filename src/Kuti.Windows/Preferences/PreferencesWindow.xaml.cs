using Kuti.Windows.Common.VirtualDesktops;
using Kuti.Windows.Preferences.Themes;
using Kuti.Windows.QuickActions;
using Kuti.Windows.VirtualDesktops;
using Serilog;
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
        private const int _expectedPageCount = 2;
        private readonly Dictionary<string, IPreferencesPage> _loadedPages = new Dictionary<string, IPreferencesPage>(_expectedPageCount);
        private readonly Dictionary<string, Func<IPreferencesPage>> _pageBuilders = new Dictionary<string, Func<IPreferencesPage>>(_expectedPageCount); 

        public PreferencesWindow()
        {
            InitializeComponent();

            settingsTree.SelectedItemChanged += (_, e) => {
                var pageTitle = e.NewValue as string;
                if (pageTitle == null) return;

                IPreferencesPage page;
                if (_loadedPages.ContainsKey(pageTitle))
                {
                    page = _loadedPages[pageTitle];
                }
                else
                {
                    var builder = _pageBuilders.GetValueOrDefault(pageTitle)
                        ?? throw new InvalidOperationException($"'{pageTitle}' is not a known Preference Page");
                    _loadedPages[pageTitle] = page = builder();
                }

                contentPane.Content = page;
                page.OnShow();
                titleBox.Text = pageTitle;
            };

            var runtime = Runtime.Current;

            _pageBuilders.Add(
                "Desktop Assignments", 
                () => new AppToDesktopMappingsPage(
                        runtime.GetInstance<IPreferencesDb>(),
                        runtime.GetInstance<IDesktopsManager>(), 
                        runtime.GetInstance<ILogger>())
                );

            _pageBuilders.Add(
                "Hotkey Settings",
                () => new HotkeysPage(runtime.GetInstance<IHotkeyManager>()));

            var sortedTitleNames = from pageTitle in _pageBuilders.Keys
                                   orderby pageTitle
                                   select pageTitle;
                                
            foreach (var page in sortedTitleNames) 
            {
                settingsTree.Items.Add(page);
            }

            if (settingsTree.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem defaultPage)
            {
                defaultPage.IsSelected = true;
            }
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
