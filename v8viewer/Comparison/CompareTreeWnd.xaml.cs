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

        private void twTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (sender is Label && ((Label)sender).Content is ComparisonSide)
            {

                var cmpSide = ((Label)sender).Content as ComparisonSide;
                var editable = cmpSide.Object as Editors.IEditable;

                if (editable != null)
                {
                    var editor = editable.GetEditor();

                    Action showAction = () =>
                        {
                            editor.Edit(this);
                        };

                    Dispatcher.BeginInvoke(showAction);

                    e.Handled = true;
                }

            }
        }

    }


    

}
