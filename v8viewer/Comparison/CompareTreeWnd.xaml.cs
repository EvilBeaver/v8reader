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
            double percent = Math.Round(HeaderGrid.ActualWidth * 40 / 100, 2);
            HeaderGrid.ColumnDefinitions[0].Width = new GridLength(percent, GridUnitType.Pixel);
            HeaderGrid.ColumnDefinitions[1].Width = new GridLength(percent, GridUnitType.Pixel);
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
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

        private void GatherUICommands(ComparisonItem cmpItem, List<UICommand> RClickCommands, bool IsLeftSourced)
        {
            if (cmpItem == null || cmpItem.NodeType == ResultNodeType.ObjectsCollection)
            {
                return;
            }

            if (cmpItem.NodeType == ResultNodeType.FakeNode)
            {
                GatherUICommands(cmpItem.Parent, RClickCommands, IsLeftSourced);
                return;
            }

            // gathering treeitem commands
            object srcObject = (IsLeftSourced) ? cmpItem.Left.Object : cmpItem.Right.Object;

            if (cmpItem.NodeType == ResultNodeType.PropertyDef)
            {
                if (srcObject != null)
                {
                    var PropDef = (PropDef)srcObject;
                    AddProviderCommands(PropDef as ICommandProvider, RClickCommands);

                    if (cmpItem.Status == ComparisonStatus.Modified || cmpItem.Status == ComparisonStatus.Match)
                    {
                        if (PropDef.Value is V8ModuleProcessor)
                        {
                            var cmd = new UICommand("Показать различия в модулях", cmpItem, new Action(
                                () =>
                                {
                                    V8ModuleProcessor LeftVal = ((PropDef)cmpItem.Left.Object).Value as V8ModuleProcessor;
                                    V8ModuleProcessor RightVal = ((PropDef)cmpItem.Right.Object).Value as V8ModuleProcessor;
                                    var viewer = LeftVal.GetDifferenceViewer(RightVal);
                                    viewer.ShowDifference();
                                }));

                            RClickCommands.Add(cmd);
                        }
                    }
                    GatherUICommands(cmpItem.Parent, RClickCommands, IsLeftSourced);
                }
            }
            else if (cmpItem.NodeType == ResultNodeType.Object)
            {
                var Provider = srcObject as ICommandProvider;
                AddProviderCommands(Provider, RClickCommands);
            }

        }

        private void AddProviderCommands(ICommandProvider Provider, List<UICommand> RClickCommands)
        {
            if (Provider != null && Provider.Commands != null)
            {
                RClickCommands.AddRange(Provider.Commands);
            }
        }

        private void Label_MouseRightButtonUp_1(object sender, MouseButtonEventArgs e)
        {

            List<UICommand> RClickCommands = new List<UICommand>();
            
            var LabelContentObject = ((Label)sender).Tag;
            
            Border sideHolder = Utils.UIHelper.FindLogicalParent<Border>((Label)sender);
            bool isLeft;
            if (sideHolder.Name == "LeftSide")
            {
                isLeft = true;
            }
            else if (sideHolder.Name == "RightSide")
            {
                isLeft = false;
            }
            else
            {
                var TreeObject = LabelContentObject as ICommandProvider;
                if (TreeObject != null && TreeObject.Commands != null)
                {
                    RClickCommands.AddRange(TreeObject.Commands);
                }

                return;
            }

            TreeViewItem twItem = Utils.UIHelper.FindVisualParent<TreeViewItem>(sideHolder);
            if (twItem != null && twItem.Header is ComparisonItem)
            {
                var cmpItem = (ComparisonItem)twItem.Header;
                GatherUICommands(cmpItem, RClickCommands, isLeft);
            }

            ShowCommandsPopup(RClickCommands);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }


    

}
