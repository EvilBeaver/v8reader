using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using V8Reader.Editors;

namespace V8Reader.Comparison
{
    class EditableObjectSection : Section
    {
        private IEditable _editableObject;

        public EditableObjectSection(IEditable EditableObject, Inline Text)
        {
            _editableObject = EditableObject;

            var link = new Hyperlink(Text);
            link.Click += link_Click;

            var holder = new Paragraph(link) { Margin = new System.Windows.Thickness(0) };
            this.Blocks.Add(holder);

        }

        void link_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var editor = _editableObject.GetEditor();
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    editor.Edit();

                }));
        }
    }
}
