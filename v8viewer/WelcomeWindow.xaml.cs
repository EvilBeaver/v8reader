using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using V8Reader.Core;
using V8Reader.Editors;
using V8Reader.Comparison;

namespace V8Reader
{
    /// <summary>
    /// Interaction logic for WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            var mainWnd = new SettingsWindow();
            mainWnd.Owner = this;
            mainWnd.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            V8Reader.Properties.Settings.Default.ShowWelcomeScreen = (bool)ShowOnStartup.IsChecked;
            V8Reader.Properties.Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowOnStartup.IsChecked = V8Reader.Properties.Settings.Default.ShowWelcomeScreen;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Внешняя обработка (*.epf)|*.epf";
            if((bool)dlg.ShowDialog(this))
            {
                MDDataProcessor proc = MDDataProcessor.Create(dlg.FileName);
                ICustomEditor editor = proc.GetEditor();
                editor.Edit();
            }
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var mainWnd = new SettingsWindow();
            mainWnd.Owner = this;
            mainWnd.Show();
        }
    }
}
