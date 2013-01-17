using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class V8ModuleProcessor : Editors.IEditable, Comparison.IComparableItem
    {
        public V8ModuleProcessor(string text)
        {

        }

        private string m_Text;

        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        #region IEditable Members

        public Editors.ICustomEditor GetEditor()
        {
            return new Editors.ModuleEditor(this, true);
        }

        #endregion

        #region IComparableItem Members

        public bool CompareTo(object Comparand)
        {
            return Text == (string)Comparand;
        }

        #endregion
    }
}
