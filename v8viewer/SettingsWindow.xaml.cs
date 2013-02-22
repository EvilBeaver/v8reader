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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace V8Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void btnBrowseWorkshop_Click(object sender, RoutedEventArgs e)
        {
            var Dlg = new Microsoft.Win32.OpenFileDialog();
            Dlg.Filter = "Приложение |*.exe";
            Dlg.Multiselect = false;
            if ((bool)Dlg.ShowDialog(this))
            {
                txtFileWorkshopPath.Text = Dlg.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtDiffCmdLine.Text = V8Reader.Properties.Settings.Default.DiffCmdLine;
            
            txtFileWorkshopPath.Text = V8Reader.Properties.Settings.Default.PathToFileWorkshop;
            txtFileWorkshopPath.Focus();

            epfAssociation.IsChecked = Utils.FileAssociation.IsAssociated(".epf", "V8Viewer.DataProcessor");

            VersionLbl.Inlines.Add("Версия: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

        }

        private void cmdОК_Click(object sender, RoutedEventArgs e)
        {

            bool success = true;
            
            try
            {
                V8Reader.Properties.Settings.Default.PathToFileWorkshop = txtFileWorkshopPath.Text;
                V8Reader.Properties.Settings.Default.DiffCmdLine = txtDiffCmdLine.Text;
                V8Reader.Properties.Settings.Default.Save();
                HandleAssociation(".epf", epfAssociation);
            }
            catch (System.Security.SecurityException)
            {
                success = false;
            }
            catch (UnauthorizedAccessException)
            {
                success = false;
            }

            if (success)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Для изменения файловых ассоциаций требуются права администратора.", "Отказ в доступе", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void cmdCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            String url = @"http://v8.1c.ru/metod/fileworkshop.htm";

            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

        }

        private void HandleAssociation(String extension, CheckBox checkBox)
        {
            if ((bool)checkBox.IsChecked)
            {
                StringBuilder sb = new StringBuilder(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                sb.Insert(0, '"');
                sb.Append('"');

                String exeName = sb.ToString();

                Utils.FileAssociation.Associate(extension, FileAssociationProgID, "Внешняя обработка 1С", exeName + ",0", exeName);
            }
            else
            {
                Utils.FileAssociation.RemoveAssociation(extension, FileAssociationProgID);
            }
        }

        private const String FileAssociationProgID = "V8Viewer.DataProcessor";

        private void btnBrowseDiff_Click(object sender, RoutedEventArgs e)
        {
            var Dlg = new Microsoft.Win32.OpenFileDialog();
            Dlg.Filter = "Приложение |*.exe";
            Dlg.Multiselect = false;
            if ((bool)Dlg.ShowDialog(this))
            {
                txtDiffCmdLine.Text = Dlg.FileName;
            }
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var answer = MessageBox.Show("Проверить обновления?", "V8 Viewer", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                var UpdChecker = new Utils.UpdateChecker();

                if (UpdChecker.HasUpdates())
                {
                    var Log = UpdChecker.GetLog();
                }

            }
            catch (Utils.UpdaterException wexc)
            {
                MessageBox.Show("Ошибка проверки обновлений:\n" + wexc.ToString(), "V8 Viewer", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception exc)
            {
                Utils.UIHelper.DefaultErrHandling(exc);
            }

        }

    }
}
