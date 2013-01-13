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
    /// Логика взаимодействия для ManagedForm.xaml
    /// </summary>
    partial class ManagedFormWnd : Window
    {
       
        internal ManagedFormWnd(MDManagedForm frm)
        {
            InitializeComponent();
            m_EditedForm = frm;
        }

        private MDManagedForm m_EditedForm;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = m_EditedForm.Name;

            ModuleCode.Text = m_EditedForm.Module;

            StaticTreeNode ElemRoot = new StaticTreeNode("Форма", IconCollections.ManagedForm["Form"], m_EditedForm.Elements);

            Elements.Items.Add(ElemRoot);
            var twItem = (TreeViewItem)Elements.ItemContainerGenerator.ContainerFromItem(ElemRoot);
            twItem.IsExpanded = true;

            if(m_EditedForm.Attributes != null)
            {
                foreach (var attrib in m_EditedForm.Attributes)
                {
                    Attributes.Items.Add(attrib);
                }
            }
            
        }
    }
}
