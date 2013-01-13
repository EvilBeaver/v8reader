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

namespace V8Reader
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DiffFilesSelect : Window
    {
        public DiffFilesSelect()
        {
            InitializeComponent();
        }

        public string FirstFile 
        {
            get
            {
                return tbFirstFile.Text;
            }
        }

        public string SecondFile
        {
            get
            {
                return tbSecondFile.Text;
            }
        }

        private void btnBrowseFirst_Click(object sender, RoutedEventArgs e)
        {
            SelectFile(tbFirstFile);
        }

        private void btnBrowseSecond_Click(object sender, RoutedEventArgs e)
        {
            SelectFile(tbSecondFile);
        }

        private void SelectFile(TextBox tbDestination)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Filter = "Внешние обработки (*.epf)|*.epf";

            try
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(tbDestination.Text);
            }
            catch
            { // в текстбоксе написана ерунда. ничего не поделаешь
            }
            
            dlg.Multiselect = false;

            if ((bool)dlg.ShowDialog())
            {
                tbDestination.Text = dlg.FileName;
            }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

    }
}
