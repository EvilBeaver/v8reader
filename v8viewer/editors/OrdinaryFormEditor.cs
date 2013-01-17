using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Editors
{
    class OrdinaryFormEditor : CustomEditor, ICustomEditor
    {
        public OrdinaryFormEditor(MDOrdinaryForm EditedForm):base()
        {
            m_EditedForm = EditedForm;
        }

        public void Edit()
        {
            Edit(null);
        }

        public void Edit(System.Windows.Window Owner)
        {
            var frm = new CodeEditorWnd();

            frm.Title = m_EditedForm.Name + ": Модуль формы";
            frm.Owner = Owner;
            frm.codeTextBox.Text = m_EditedForm.Module;
            frm.Show();

        }

        private MDOrdinaryForm m_EditedForm;

    }
}
