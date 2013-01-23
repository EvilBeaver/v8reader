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

        private void twElements_RightMouseUp(object sender, MouseButtonEventArgs e)
        {
            Label lblSender = sender as Label;
            if (lblSender != null)
            {
                

                Dictionary<String, bool> visibilityMask = new Dictionary<string, bool>();

                visibilityMask.Add("mnuOpen", lblSender.Tag is Editors.IEditable);
                visibilityMask.Add("mnuHelp", lblSender.Tag is MDForm);

                int visibleItems = SetFormMenusVisibility(lblSender.ContextMenu, visibilityMask);

                if (visibleItems > 0)
                {
                    lblSender.ContextMenu.Tag = lblSender.Tag;
                    lblSender.ContextMenu.IsOpen = true;
                }

                e.Handled = true;

            }
        }

        private int SetFormMenusVisibility(ContextMenu mnu, Dictionary<String, bool> visibilityMask)
        {
            int visibleCount = 0;
            foreach (MenuItem item in mnu.Items)
            {

                bool Visible, hasValue;
                hasValue = visibilityMask.TryGetValue(item.Name, out Visible);
                if (!hasValue)
                {
                    Visible = item.IsVisible;
                }

                System.Windows.Visibility vis;
                if (Visible)
                {
                    vis = System.Windows.Visibility.Visible;
                    visibleCount++;
                }
                else
                    vis = System.Windows.Visibility.Collapsed;

                item.Visibility = vis;

            }

            return visibleCount;

        }

        private void mnuHelp_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu Menu = ((MenuItem)sender).Parent as ContextMenu;
            if (Menu == null)
                return;

            MDForm form = Menu.Tag as MDForm;
            if (form != null && !form.Help.IsEmpty)
            {
                try
                {
                    String Path = form.Help.Location;
                    System.Diagnostics.Process.Start(Path);
                }
                catch (Exception exc)
                {
                    Utils.UIHelper.DefaultErrHandling(exc);
                }
            }
        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu Menu = ((MenuItem)sender).Parent as ContextMenu;
            if (Menu == null)
                return;

            Editors.IEditable editable = Menu.Tag as Editors.IEditable;
            if (editable == null)
                return;

            var editor = editable.GetEditor();
            editor.Edit();

            e.Handled = true;
        }

        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            var editable = ((Label)sender).Tag as Editors.IEditable;

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

    }


    

}
