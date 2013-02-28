using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    abstract class MDForm : MDBaseObject, IMDTreeItem, ICommandProvider, Editors.IEditable
    {
        public enum FormKind
        {
            Ordinary,
            Managed
        }

        public FormKind Kind { get; protected set; }

        public HTMLDocument Help
        {
            get
            {

                if (m_Help == null)
                {
                    try
                    {
                        var HelpItem = m_Reader.GetElement(ID + ".1");
                        var Stream = new SerializedList(HelpItem.ReadAll());

                        m_Help = new HTMLDocument(Stream);

                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        m_Help = new HTMLDocument();
                    }
                }

                return m_Help;

            }
        }
        public abstract string Module { get; }
        public abstract MDUserDialogBase DialogDef { get; }

        protected MDForm(String ObjID, MDReader Reader) : base(ObjID)
        {
            m_Reader = Reader;

            MDFileItem header = Reader.GetElement(ID);

            SerializedList StringsBlock = FindStringsBlock(header.ReadAll());

            MDBaseObject.ReadStringsBlock(this, StringsBlock);

        }

        protected SerializedList FindStringsBlock(String RawContent)
        {
            int pos = RawContent.IndexOf("{0,0," + this.ID + "}");
            int ListStart = -1;
            if (pos > 0)
            {
                for (int j = pos - 1; RawContent[j] != '{' && j >= 0; --j)
                {
                    ListStart = j;
                }
            }
            else if (pos == 0)
            {
                ListStart = 0;
            }
            else
                throw new MDStreamFormatException();

            if (ListStart < 0)
                throw new MDStreamFormatException();

            return new SerializedList(RawContent.Substring(ListStart - 1));
        }

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            var internalProps = base.PropHolder;

            internalProps.Add(PropDef.Create("Dialog", "Форма", DialogDef, new Comparison.ToStringComparator()));
            internalProps.Add(PropDef.Create("Module", "Модуль", new V8ModuleProcessor(Module, Name)));
            
        }

        protected MDReader m_Reader;
        private HTMLDocument m_Help = null;

        ////////////////////////////////////////////////////////
        // static

        public static MDForm CreateByID(String ObjID, MDReader Reader)
        {
            var Container = Reader.GetElement(ObjID + ".0");

            if (Container.ElemType == MDFileItem.ElementType.Directory)
            {
                return new MDOrdinaryForm(ObjID, Reader);
            }
            else
            {
                return new MDManagedForm(ObjID, Reader);
            }
        }

        #region IMDTreeItem implementation

        public virtual string Key
        {
            get { return ID; }
        }

        public virtual string Text
        {
            get { return Name; }
        }

        public virtual AbstractImage Icon
        {
            get { return  IconCollections.MDObjects["Form"]; }
        }

        public virtual bool HasChildren()
        {
            return false;
        }

        public virtual IEnumerable<IMDTreeItem> ChildItems
        {
            get { return null; }
        }

        public virtual IEnumerable<UICommand> Commands
        {
            get
            {
                List<UICommand> cmdList = new List<UICommand>();

                cmdList.Add(new UICommand("Открыть", this, new Action(()=>
                    {
                        var editor = ((Editors.IEditable)this).GetEditor();
                        editor.Edit();

                    })));

                cmdList.Add(new UICommand("Справочная информация", this, new Action(() =>
                {
                    if (!Help.IsEmpty)
                    {
                        String Path = Help.Location;
                        System.Diagnostics.Process.Start(Path);
                        
                    }

                })));

                return cmdList;
            }
        }

        #endregion

        #region IEditable Members

        public virtual Editors.ICustomEditor GetEditor()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal enum FormElementClass
    {
        Field,
        Label,
        Button,
        Grid,
        Group,
        Unknown
    }

    abstract class FormElement : NamedObject
    {
        public FormElement(String ElementName, String Title, FormElementClass ElemClass)
            : base(ElementName, Title)
        {
            m_Class = ElemClass;
        }

        public FormElementClass Class 
        { 
            get { return m_Class; }
        }

        private FormElementClass m_Class;

    }
}
