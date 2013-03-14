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

            _ProcessingAssociation = new FileAssociation();
            _ProcessingAssociation.Extension = ".epf";
            _ProcessingAssociation.ProgID = "V8Viewer.DataProcessor";
            _ProcessingAssociation.IconIndex = "0";

            _ReportAssociation = new FileAssociation();
            _ReportAssociation.Extension = ".erf";
            _ReportAssociation.ProgID = "V8Viewer.Report";
            _ReportAssociation.IconIndex = "1";

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

            epfAssociation.IsChecked = ReadAssociation(_ProcessingAssociation);
            erfAssociation.IsChecked = ReadAssociation(_ReportAssociation);

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
                WriteAssociation(_ProcessingAssociation, epfAssociation);
                WriteAssociation(_ReportAssociation, erfAssociation);
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

        private bool ReadAssociation(FileAssociation assoc)
        {
            return Utils.FileAssociation.IsAssociated(assoc.Extension, assoc.ProgID);
        }

        private void WriteAssociation(FileAssociation assoc, CheckBox checkBox)
        {
            if ((bool)checkBox.IsChecked)
            {
                StringBuilder sb = new StringBuilder(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                sb.Insert(0, '"');
                sb.Append('"');

                String exeName = sb.ToString();

                string Value = String.Format("{0},{1}", exeName, assoc.IconIndex);

                Utils.FileAssociation.Associate(assoc.Extension, assoc.ProgID, "Внешняя обработка 1С", Value, exeName);
            }
            else
            {
                Utils.FileAssociation.RemoveAssociation(assoc.Extension, assoc.ProgID);
            }
        }

        private FileAssociation _ProcessingAssociation;
        private FileAssociation _ReportAssociation;

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

            var UpdChecker = new Utils.UpdateChecker();

            try
            {
                UpdChecker.CheckUpdates(UpdateCheckerCallback);
            }
            catch (Exception exc)
            {
                Utils.UIHelper.DefaultErrHandling(exc);
            }

        }

        void UpdateCheckerCallback(Utils.UpdateChecker uc, Utils.UpdateCheckerResult result)
        {
            try
            {
                
                if (!result.Success) throw result.Exception;
                            
                if (result.Updates.Count > 0)
                {
                    var UpdWnd = new Utils.UpdatesWnd();
                    UpdWnd.Updates = result.Updates;
                    UpdWnd.Owner = this;
                    UpdWnd.Show();
                }
                else
                {
                    MessageBox.Show("Новых версий нет.", "V8 Viewer", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
            catch (System.Net.WebException webExc)
            {
                MessageBox.Show(webExc.ToString());
                return;
            }
            catch (Exception exc)
            {
                Utils.UIHelper.DefaultErrHandling(exc);
            }
        }

        private struct FileAssociation
        {
            public string ProgID;
            public string Extension;
            public string IconIndex;
        }

    }
}
