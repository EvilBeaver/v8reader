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

        private bool SelectFile(TextBox tbDestination)
        {
            var dlg = Utils.UIHelper.GetOpenFileDialog();

            if (tbDestination == tbFirstFile)
                dlg.Title = "Выберите первый файл";
            else if (tbDestination == tbSecondFile)
                dlg.Title = "Выберите второй файл";


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
                return true;
            }

            return false;

        }

        private void PerformSelection()
        {
            DialogResult = true;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            PerformSelection();
        }

        private void Window_ContentRendered_1(object sender, EventArgs e)
        {
            tbFirstFile.Focus();
            
            if (tbFirstFile.Text == String.Empty && tbSecondFile.Text == String.Empty)
            {
                if (SelectFile(tbFirstFile) && SelectFile(tbSecondFile))
                {
                    PerformSelection();
                }
            }
        }

    }
}
