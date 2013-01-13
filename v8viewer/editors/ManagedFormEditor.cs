using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Core;

namespace V8Reader.Editors
{
    class ManagedFormEditor : CustomEditor, ICustomEditor
    {
        public ManagedFormEditor(MDManagedForm frm) : base()
        {
            m_EditedForm = frm;
        }

        public void Edit()
        {
            Edit(null);
        }

        public void Edit(System.Windows.Window Owner)
        {
            var form = new ManagedFormWnd(m_EditedForm);
            form.Owner = Owner;
            form.Closed += new EventHandler(form_Closed);
            form.Show();
        }

        void form_Closed(object sender, EventArgs e)
        {
            OnEditComplete(true, m_EditedForm);
        }

        private MDManagedForm m_EditedForm;
    }
}
