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

namespace V8Reader.Comparison
{
    /// <summary>
    /// Interaction logic for FlowDocViewer.xaml
    /// </summary>
    public partial class FlowDocViewer : Window
    {
        public FlowDocViewer()
        {
            InitializeComponent();
        }

        public FlowDocument Document 
        {
            get
            {
                return fdViewer.Document;
            }
            set
            {
                fdViewer.Document = value;
            }
        }

    }
}
