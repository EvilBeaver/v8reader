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
            var dlg = Utils.UIHelper.GetOpenFileDialog();
            
            if ((bool)dlg.ShowDialog(this))
            {
                V8MetadataContainer Container = new V8MetadataContainer(dlg.FileName);

                IEditable editable = Container.RaiseObject() as IEditable;
                if (editable == null)
                {
                    MessageBox.Show("Редактирование данного объекта не поддерживается", "V8 Reader", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Container.Dispose();
                    return;
                }

                ICustomEditor editor = editable.GetEditor();
                editor.EditComplete += (s, evArg) =>
                    {
                        Container.Dispose();
                    };

                editor.Edit();
            }
        }

        private void btnDiff_Click(object sender, RoutedEventArgs e)
        {

            DiffFilesSelect selector = new DiffFilesSelect();
            selector.Owner = this;

            if ((bool)selector.ShowDialog())
            {
                string file1 = selector.FirstFile;
                string file2 = selector.SecondFile;


                FileComparisonPerformer Comparator = null;
                try
                {
                    Comparator = new FileComparisonPerformer(file1, file2);
                    
                    var TreeWnd = new CompareTreeWnd();

                    TreeWnd.LeftName = System.IO.Path.GetFileName(file1);
                    TreeWnd.RightName = System.IO.Path.GetFileName(file2);
                    TreeWnd.PrintResult(Comparator);
                    TreeWnd.Show();
                    
                }
                catch (System.IO.FileNotFoundException exc)
                {
                    if (Comparator != null) Comparator.Dispose();
                    MessageBox.Show(exc.ToString(), "Неверное имя файла", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                catch (Exception exc)
                {
                    if (Comparator != null) Comparator.Dispose();
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
