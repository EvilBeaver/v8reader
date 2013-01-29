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

        private List<UICommand> RClickCommands;

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

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }

        private void TreeViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var cmpItem = ((TreeViewItem)sender).Header as ComparisonItem;

            SetItemCommands(cmpItem);
            ShowCommandsPopup(RClickCommands);
            e.Handled = true;

        }

        private void SetItemCommands(ComparisonItem CurrentItem)
        {
            if (RClickCommands == null)
            {
                RClickCommands = new List<UICommand>();
            }

            if (CurrentItem.Left != null && CurrentItem.Left.Object is PropDef)
            {

            }

        }

        private void ShowCommandsPopup(List<UICommand> Commands)
        {
            if (Commands == null || Commands.Count == 0)
            {
                return;
            }

            var Menu = new ContextMenu();
            TextOptions.SetTextFormattingMode(Menu, TextFormattingMode.Display);

            foreach (var Command in Commands)
            {
                MenuItem item = new MenuItem();
                item.Header = Command;

                item.Click += (s, e) =>
                {
                    try
                    {
                        Command.Execute(this);
                    }
                    catch (Exception exc)
                    {
                        Utils.UIHelper.DefaultErrHandling(exc);
                    }
                };

                Menu.Items.Add(item);

            }

            Menu.IsOpen = true;

        }

        private void Label_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e)
        {

            RClickCommands = new List<UICommand>();

            var TreeObject = ((Label)sender).Tag as ICommandProvider;
            if (TreeObject != null && TreeObject.Commands != null)
            {
                RClickCommands.AddRange(TreeObject.Commands);
            }
            

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }


    

}
