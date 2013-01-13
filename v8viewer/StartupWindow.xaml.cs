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
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Внешняя обработка (*.epf)|*.epf";
            if ((bool)dlg.ShowDialog(this))
            {
                MDDataProcessor proc = MDDataProcessor.Create(dlg.FileName);
                ICustomEditor editor = proc.GetEditor();
                editor.EditComplete += new EditorCompletionHandler(editor_EditComplete);
                editor.Edit();
            }
        }

        void editor_EditComplete(object Sender, EditorEventArgs e)
        {
            MDDataProcessor proc = e.EditedObject as MDDataProcessor;
            proc.Dispose();
        }

        private void btnDiff_Click(object sender, RoutedEventArgs e)
        {

            DiffFilesSelect selector = new DiffFilesSelect();
            selector.Owner = this;

            if ((bool)selector.ShowDialog())
            {
                string file1 = selector.FirstFile;
                string file2 = selector.SecondFile;

                try
                {
                    using (FileComparisonPerformer Comparator = new FileComparisonPerformer(file1, file2))
                    {
                        var CompareTree = Comparator.Perform();
                        var TreeWnd = new CompareTreeWnd();

                        TreeWnd.PrintResult(CompareTree);
                        TreeWnd.Show();
                    }
                }
                catch (System.IO.FileNotFoundException exc)
                {
                    MessageBox.Show(exc.ToString(), "Неверное имя файла", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch (Exception exc)
                {
                    Utils.UIHelper.DefaultErrHandling(exc);
                }

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
