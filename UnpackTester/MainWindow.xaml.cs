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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UnpackTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Внешняя обработка (*.epf)|*.epf";
            if ((bool)dlg.ShowDialog(this))
            {
                FilePath.Text = dlg.FileName;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (V8Unpack.V8File file = new V8Unpack.V8File(FilePath.Text))
            {
                var pages = file.GetLister().Items;
            }
        }
    }
}
