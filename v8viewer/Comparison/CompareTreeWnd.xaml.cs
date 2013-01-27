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

namespace V8Reader.Comparison
{
    /// <summary>
    /// Interaction logic for CompareTreeWnd.xaml
    /// </summary>
    public partial class CompareTreeWnd : Window
    {
        internal CompareTreeWnd()
        {
            InitializeComponent();
            Utils.FormsSettingsManager.Register(this, "ComparisonTree");
        }

        internal void PrintResult(IComparisonPerformer Performer)
        {
            m_Engine = Performer;

            PrintResultInternal();

        }

        private void PrintResultInternal()
        {

            twTree.Items.Clear();

            ComparisonPerformer.MatchingMode mode;
            if ((bool)chkMatchNames.IsChecked)
            {
                mode = ComparisonPerformer.MatchingMode.ByName;
            }
            else
            {
                mode = ComparisonPerformer.MatchingMode.ByID;
            }

            var Result = m_Engine.Perform(mode);
            twTree.Items.Add(Result);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (m_Engine != null)
            {
                PrintResultInternal();
            }
        }

        IComparisonPerformer m_Engine;

        

        private void CompareTree_Loaded(object sender, RoutedEventArgs e)
        {
            var twItem = (TreeViewItem)twTree.ItemContainerGenerator.ContainerFromIndex(0);
            twItem.IsExpanded = true;

        }

        private void CompareTree_ContentRendered(object sender, EventArgs e)
        {
            double percent = Math.Round(HeaderGrid.ActualWidth * 35 / 100, 2);
            HeaderGrid.ColumnDefinitions[0].Width = new GridLength(percent, GridUnitType.Pixel);
            HeaderGrid.ColumnDefinitions[1].Width = new GridLength(percent, GridUnitType.Pixel);
        }

        private void TreeViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var twSender = ((TreeViewItem)sender).IsSelected = true;
        }

    }


    

}
