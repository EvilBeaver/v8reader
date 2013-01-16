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
    /// Логика взаимодействия для DataProcEditorWnd.xaml
    /// </summary>
    partial class DataProcEditorWnd : Window
    {
        
        internal DataProcEditorWnd(MDDataProcessor EditedObject)
        {
            InitializeComponent();
            Utils.FormsSettingsManager.Register(this, "DataProcessor");
            m_Object = (MDDataProcessor)EditedObject;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtName.Text = m_Object.Name;
            txtSynonym.Text = m_Object.Synonym;
            txtComment.Text = m_Object.Comment;

            foreach (IMDTreeItem node in m_Object.ChildItems)
            {
                twElements.Items.Add(node);
            }

        }

        MDDataProcessor m_Object;


        private void btnObjectModule_Click(object sender, RoutedEventArgs e)
        {
            // module search

            string module = m_Object.ObjectModule;

            if(module == null)
            {
                MessageBox.Show(this, "Модуль объекта отсутствует", m_Object.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            var frmEditor = new CodeEditor();
            frmEditor.codeTextBox.Text = module;
            frmEditor.Title = "Модуль объекта: " + m_Object.Name;
            frmEditor.Show();

        }

        private void twElements_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TreeViewItem && ((TreeViewItem)e.Source).IsSelected)
            {
                e.Handled = true; // а то всплывет вверх по дереву

                var twi = (TreeViewItem)e.Source;

                var Editable = twi.Header as IEditable;
                if (Editable != null)
                {
                    try
                    {

                        var waitingEditor = Editable.GetEditor();

                        Action showAction = () => waitingEditor.Edit();
                        this.Dispatcher.BeginInvoke(showAction);

                    }
                    catch (Exception exc)
                    {
                        DefaultErrHandling(exc);
                    }

                }

            }
            else
            {
                e.Handled = true;
            }
        }

        private void twElements_RightMouseUp(object sender, MouseButtonEventArgs e)
        {

            TreeViewItem twSender = sender as TreeViewItem;
            if (twSender != null)
            {
                twSender.IsSelected = true;

                Dictionary<String, bool> visibilityMask = new Dictionary<string, bool>();

                visibilityMask.Add("mnuOpen", twSender.Header is IEditable);
                visibilityMask.Add("mnuHelp", twSender.Header is MDForm);

                int visibleItems = SetFormMenusVisibility(twSender.ContextMenu, visibilityMask);

                if (visibleItems > 0)
                {
                    twSender.ContextMenu.Tag = twSender.Header;
                    twSender.ContextMenu.IsOpen = true;
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

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            var Help = m_Object.Help;
            if (!Help.IsEmpty)
            {
                try
                {
                    String Path = Help.Location;
                    System.Diagnostics.Process.Start(Path);
                }
                catch (Exception exc)
                {
                    DefaultErrHandling(exc);
                }
            }
        }

        private void btnActions_Click(object sender, RoutedEventArgs e)
        {
            btnActionsPopup.PlacementTarget = (UIElement)sender;
            btnActionsPopup.IsOpen = true;

        }

        private void btnActions_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var mainWnd = new SettingsWindow();
            mainWnd.Owner = this;
            mainWnd.Show();
        }

        private void mnuCompareTo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Внешняя обработка (*.epf)|*.epf";
            if ((bool)dlg.ShowDialog(this))
            {
                MDDataProcessor proc = MDDataProcessor.Create(dlg.FileName);

                Comparison.ComparisonPerformer Performer 
                    = new Comparison.ComparisonPerformer((IMDTreeItem)m_Object, (IMDTreeItem)proc);

                var diffWnd = new Comparison.CompareTreeWnd();
                diffWnd.PrintResult(Performer);
                diffWnd.Owner = this;
                diffWnd.Show();

            }
        }

        private void DefaultErrHandling(Exception exc)
        {
            Utils.UIHelper.DefaultErrHandling(exc);
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
                    DefaultErrHandling(exc);
                }
            }
        }

        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu Menu = ((MenuItem)sender).Parent as ContextMenu;
            if (Menu == null)
                return;

            IEditable editable = Menu.Tag as IEditable;
            if (editable == null)
                return;

            var editor = editable.GetEditor();
            editor.Edit();

            e.Handled = true;
        }

    }
}
