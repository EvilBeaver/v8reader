using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Editors
{
    class BinaryTemplateEditor : CustomEditor, ICustomEditor
    {

        public BinaryTemplateEditor(BinaryDataDocument Document)
        {
            m_Document = Document;
        }

        public void Edit()
        {
            Edit(null);
        }

        public void Edit(System.Windows.Window Owner)
        {
            var frm = new BinaryTemplateWindow(m_Document);
            frm.Owner = Owner;
            frm.Show();
        }

        private BinaryDataDocument m_Document;
    }
}
