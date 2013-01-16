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

    }


    

}
