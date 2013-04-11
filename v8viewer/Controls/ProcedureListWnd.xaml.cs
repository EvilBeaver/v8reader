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

namespace V8Reader.Controls
{
    /// <summary>
    /// Interaction logic for ProcedureListWnd.xaml
    /// </summary>
    public partial class ProcedureListWnd : Window
    {
        internal ProcedureListWnd(IList<ProcListItem> ProcList)
        {
            InitializeComponent();

            _procList = ProcList;

            //_cw = new CollectionViewSource();
            //_cw.Source = _procList;
            //lbProcList.ItemsSource = _cw.View;

            lbProcList.ItemsSource = _procList;
        }

        private IList<ProcListItem> _procList;
        //private CollectionViewSource _cw;

        private void txtProcName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var FilterPredicate = new Predicate<object>(objItem => 
                {
                    var item = objItem as ProcListItem;

                    string text = (sender as TextBox).Text;
                    if (text == "")
                    {
                        return true;
                    }
                    else
                    {
                        return item.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase);
                    }
                });
            
            lbProcList.Items.Filter = FilterPredicate;
            //_cw.View.Filter = FilterPredicate;

        }

        private void txtProcName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                lbProcList.Focus();
            }
        }

        private void lbProcList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void lbProcList_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void chkSort_Click(object sender, RoutedEventArgs e)
        {
            if (chkSort.IsChecked == true)
            {

                var sorted = from lst in _procList orderby lst.Name select lst;
                lbProcList.ItemsSource = sorted.ToList<ProcListItem>();
                
            }
            else
            {
                lbProcList.ItemsSource = _procList;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {

        }


        class itemsComparer : System.Collections.IComparer
        {

            #region IComparer Members

            public int Compare(object x, object y)
            {
                return String.Compare((string)x, (string)y, true);
            }

            #endregion
        }

    }
}
