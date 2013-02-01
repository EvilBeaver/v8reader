using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDOrdinaryForm : MDForm, IMDTreeItem
    {
        public MDOrdinaryForm(String ObjID, MDReader Reader) : base(ObjID, Reader) { }

        public override ICustomEditor GetEditor()
        {
            return new Editors.OrdinaryFormEditor(this);
        }

        override public String Module
        {
            get
            {
                var DirElem = m_Reader.GetElement(this.ID + ".0");
                var textElem = DirElem.GetElement("module");

                return textElem.ReadAll();
            }
        }
    }
}
