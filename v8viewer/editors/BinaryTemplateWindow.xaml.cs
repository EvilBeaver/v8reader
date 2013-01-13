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


namespace V8Reader.Editors
{
    /// <summary>
    /// Interaction logic for BinaryTemplateWindow.xaml
    /// </summary>
    partial class BinaryTemplateWindow : Window
    {
        internal BinaryTemplateWindow(BinaryDataDocument Document)
        {
            InitializeComponent();
            m_Document = Document;
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();

            if ((bool)dlg.ShowDialog(this))
            {
                System.IO.FileStream fs = null;

                try
                {
                    using (System.IO.MemoryStream ms = (System.IO.MemoryStream)m_Document.GetStream())
                    {
                        fs = new System.IO.FileStream(dlg.FileName, System.IO.FileMode.OpenOrCreate);

                        ms.WriteTo(fs);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString(), "V8 Reader", MessageBoxButton.OK, MessageBoxImage.Stop);
                }
                
            }
        }

        private BinaryDataDocument m_Document;
    }
}
