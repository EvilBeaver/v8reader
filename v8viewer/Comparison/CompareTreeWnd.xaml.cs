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

            //LeftName = "Первый файл";
            //RightName = "Второй файл";

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

            ExpandTopLevel();

        }

        IComparisonPerformer m_Engine;



        public string LeftName
        {
            get { return (string)GetValue(LeftNameProperty); }
            set { SetValue(LeftNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftNameProperty =
            DependencyProperty.Register("LeftName", typeof(string), typeof(CompareTreeWnd), new PropertyMetadata("Первый файл"), NameValidation);



        public string RightName
        {
            get { return (string)GetValue(RightNameProperty); }
            set { SetValue(RightNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightNameProperty =
            DependencyProperty.Register("RightName", typeof(string), typeof(CompareTreeWnd), new PropertyMetadata("Второй файл"), NameValidation);

        private static ValidateValueCallback NameValidation = new ValidateValueCallback((o)=>
                {
                    return o != null;
                });

        //public string LeftName 
        //{
        //    get
        //    {
        //        return _leftName;
        //    }

        //    set
        //    {
        //        if (value != null)
        //        {
        //            _leftName = value;
        //        }
        //    }
        //}
        //public string RightName 
        //{
        //    get
        //    {
        //        return _rightName;
        //    }

        //    set
        //    {
        //        if (value != null)
        //        {
        //            _rightName = value;
        //        }
        //    } 
        //}

        //private string _leftName;
        //private string _rightName;

        private void ExpandTopLevel()
        {
            TreeViewItem root = (TreeViewItem)twTree.ItemContainerGenerator.ContainerFromIndex(0);
            if(root != null)
                root.IsExpanded = true;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (m_Engine != null)
            {
                PrintResultInternal();
            }
        }

        private void CompareTree_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void CompareTree_ContentRendered(object sender, EventArgs e)
        {
            double percent = Math.Round(HeaderGrid.ActualWidth * 40 / 100, 2);
            HeaderGrid.ColumnDefinitions[0].Width = new GridLength(percent, GridUnitType.Pixel);
            HeaderGrid.ColumnDefinitions[1].Width = new GridLength(percent, GridUnitType.Pixel);

            RedrawTree();

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
                                    if (viewer != null)
                                    {
                                        viewer.ShowDifference();
                                    }
                                }));

                            RClickCommands.Add(cmd);
                        }
                        else if (PropDef.Value is TemplateDocument)
                        {
                            var cmd = new UICommand("Показать различия в макетах", cmpItem, new Action(
                                () =>
                                {
                                    TemplateDocument LeftVal = ((PropDef)cmpItem.Left.Object).Value as TemplateDocument;
                                    TemplateDocument RightVal = ((PropDef)cmpItem.Right.Object).Value as TemplateDocument;
                                    var viewer = LeftVal.GetDifferenceViewer(RightVal);
                                    if (viewer != null)
                                    {
                                        viewer.ShowDifference();
                                    }
                                }));

                            RClickCommands.Add(cmd);
                        }
                        else if (PropDef.Value is MDUserDialogBase)
                        {
                            var cmd = new UICommand("Показать различия в диалогах", cmpItem, new Action(
                                () =>
                                {
                                    MDUserDialogBase LeftVal = ((PropDef)cmpItem.Left.Object).Value as MDUserDialogBase;
                                    MDUserDialogBase RightVal = ((PropDef)cmpItem.Right.Object).Value as MDUserDialogBase;
                                    var viewer = new Comparison.ExternalTextDiffViewer(LeftVal.ToString(), RightVal.ToString());
                                    if (viewer != null)
                                    {
                                        viewer.ShowDifference();
                                    }
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

            if (srcObject != null && srcObject is IMDPropertyProvider)
            {
                var cmd = new UICommand("Отчет по свойствам", srcObject, () =>
                {

                    PropertiesReport repGenerator;

                    if (cmpItem.Left.Object != null && cmpItem.Right.Object != null)
                    {
                        repGenerator = new PropertiesReportCompare(cmpItem.Left.Object as IMDPropertyProvider, cmpItem.Right.Object as IMDPropertyProvider);
                    }
                    else
                    {
                        repGenerator = new PropertiesReportSingle((IMDPropertyProvider)srcObject);
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            FlowDocViewer fdViewer = new FlowDocViewer();
                            fdViewer.Title = srcObject.ToString();
                            fdViewer.Document = repGenerator.GenerateReport();
                            fdViewer.Show();
                        }));
                    
                });

                RClickCommands.Add(cmd);
            }
            
        }

        private void AddProviderCommands(ICommandProvider Provider, List<UICommand> RClickCommands)
        {
            if (Provider != null && Provider.Commands != null)
            {
                RClickCommands.AddRange(Provider.Commands);
            }
        }

        private void SetFilter(bool Hide)
        {
            var rootNode = (TreeViewItem)twTree.ItemContainerGenerator.ContainerFromIndex(0);
            SetNodeFilter(rootNode, Hide);
        }

        private void SetNodeFilter(TreeViewItem node, bool Hide)
        {
            if (node == null)
                return;

            ComparisonItem nodeItem = node.Header as ComparisonItem;
            if (nodeItem != null)
            {
                if (Hide && nodeItem.Status == ComparisonStatus.Match)
                {
                    node.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    node.Visibility = System.Windows.Visibility.Visible;
                }

                IterateFiltering(node, Hide);

            }

        }

        private void IterateFiltering(TreeViewItem node, bool Hide)
        {
            for (int i = 0; i < node.Items.Count; i++)
            {
                var curNode = (TreeViewItem)node.ItemContainerGenerator.ContainerFromIndex(i);
                SetNodeFilter(curNode, Hide);
            }

        }

        private void RedrawTree()
        {
            if (!twTree.HasItems)
            {
                return;
            }
            
            ComparisonResult result = (ComparisonResult)twTree.Items[0];

            twTree.Items.Clear();
            twTree.Items.Add(result);
            SetFilter(FilterCombo.SelectedIndex == 0);

            ExpandTopLevel();

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

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RedrawTree();
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {

            Action filter = new Action(() =>
            {
                TreeViewItem node = (TreeViewItem)sender;
                SetNodeFilter(node, FilterCombo.SelectedIndex == 0);
            });

            Dispatcher.BeginInvoke(filter, System.Windows.Threading.DispatcherPriority.Background);
            e.Handled = true;
        }

    }


    

}
