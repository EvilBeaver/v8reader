using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Editors
{
    class ModuleEditor : CustomEditor, ICustomEditor
    {
        public ModuleEditor(V8ModuleProcessor module, bool ReadOnly)
        {
            m_Module = module;
            m_ReadOnlyFlag = ReadOnly;
        }

        private bool m_ReadOnlyFlag;
        private V8ModuleProcessor m_Module;

        public void Edit()
        {
            Edit(null);
        }

        public void Edit(System.Windows.Window Owner)
        {
            var frmCode = new CodeEditorWnd();
            frmCode.codeTextBox.Text = m_Module.Text;
            frmCode.Owner = Owner;
            if (!m_ReadOnlyFlag)
            {
                frmCode.Closed += frmCode_Closed;
            }
        }

        void frmCode_Closed(object sender, EventArgs e)
        {
            lock (m_Module)
            {
                m_Module.Text = ((CodeEditorWnd)sender).codeTextBox.Text;
            }

            OnEditComplete(true, m_Module);
        }

    }
}
