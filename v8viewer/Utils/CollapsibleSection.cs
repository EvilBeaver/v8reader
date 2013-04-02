using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace V8Reader.Utils
{
    class CollapsibleSection : Section
    {
        public CollapsibleSection()
        {

            CollapsibleBlocks = new List<Block>();
            Header = new Paragraph();

            m_expandCollapseToggleButton = new ToggleButton();
            m_expandCollapseToggleButton.Click += m_expandCollapseToggleButton_Click;
            m_expandCollapseToggleButton.Margin = new Thickness(0, 0, 3, 0);

            IsCollapsed = true;

            m_inlineUIContainer = new InlineUIContainer(m_expandCollapseToggleButton);
            m_inlineUIContainer.BaselineAlignment = BaselineAlignment.Center;
            m_inlineUIContainer.Cursor = Cursors.Arrow;

            Header.Inlines.Add(m_inlineUIContainer);
        
        }

        private void m_expandCollapseToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Invalidate();

            if (IsCollapsed)
            {
                m_expandCollapseToggleButton.Content = "+";
            }
            else
            {
                m_expandCollapseToggleButton.Content = "-";
            }

        }

        public bool IsCollapsed
        {
            get
            {
                return !(m_expandCollapseToggleButton.IsChecked ?? false);
            }
            set
            {
                m_expandCollapseToggleButton.IsChecked = !value;

                if (value == true)
                {
                    m_expandCollapseToggleButton.Content = "+";
                }
                else
                {
                    m_expandCollapseToggleButton.Content = "-";
                }

            }
        }

        public void Invalidate()
        {
            Blocks.Clear();

            if (CollapsibleBlocks.Count == 0)
            {
                m_expandCollapseToggleButton.IsChecked = null;
            }

            Blocks.Add(Header);

            if (!IsCollapsed)
            {
                Blocks.AddRange(CollapsibleBlocks);
            }
        }

        public Paragraph Header { get; private set; }
        public List<Block> CollapsibleBlocks { get; private set; }

        private ToggleButton m_expandCollapseToggleButton;
        private InlineUIContainer m_inlineUIContainer;
    }
}
