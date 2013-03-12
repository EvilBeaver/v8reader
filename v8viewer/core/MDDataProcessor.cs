using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    partial class MDDataProcessor : MDObjectBase, IMDTreeItem, IEditable, ICommandProvider
    {

        private MDDataProcessor() : base()
        {
            m_Attributes = new MDObjectsCollection<MDAttribute>();
            m_Tables = new MDObjectsCollection<MDTable>();
            m_Forms = new MDObjectsCollection<MDForm>();
            m_Templates = new MDObjectsCollection<MDTemplate>();
        }

        public MDObjectsCollection<MDAttribute> Attributes 
        {
            get
            {
                return m_Attributes;
            }
        }
        public MDObjectsCollection<MDTable> Tables
        {
            get
            {
                return m_Tables;
            }
        }
        public MDObjectsCollection<MDForm> Forms
        {
            get
            {
                return m_Forms;
            }
        }
        public MDObjectsCollection<MDTemplate> Templates
        {
            get
            {
                return m_Templates;
            }
        }
        
        public HTMLDocument Help
        {
            get
            {
                if (m_Help == null)
                {
                    try
                    {
                        var HelpItem = m_Container.GetElement(ID + ".1");
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

        public String ObjectModule
        {
            get
            {
                MDFileItem DirElem;

                try
                {
                    DirElem = m_Container.GetElement(this.ID + ".0");
                }
                catch (System.IO.FileNotFoundException)
                {
                    return String.Empty; // Модуля нет
                }

                if (DirElem.ElemType == MDFileItem.ElementType.Directory)
                {

                    try
                    {
                        var textElem = DirElem.GetElement("text");
                        return textElem.ReadAll();
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        return String.Empty;
                    }

                }
                else
                {
                    return DirElem.ReadAll(); // если модуль зашифрован, то будет нечитаемый текст
                }
            }
        }

        
        private IV8MetadataContainer m_Container;
        private MDObjectsCollection<MDAttribute> m_Attributes;
        private MDObjectsCollection<MDTable> m_Tables;
        private MDObjectsCollection<MDForm> m_Forms;
        private MDObjectsCollection<MDTemplate> m_Templates;
        private HTMLDocument m_Help;


        #region ITreeItem implementation

        List<IMDTreeItem> m_StaticChildren;

        public String Key
        {
            get { return ID; }
        }

        public String Text
        {
            get { return Name; }
        }

        public AbstractImage Icon
        {
            get { return  IconCollections.MDObjects["DataProcessor"]; }
        }

        public bool HasChildren()
        {
            return true;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get
            {
                if (m_StaticChildren == null)
                {
                    m_StaticChildren = new List<IMDTreeItem>();

                    m_StaticChildren.Add(new StaticTreeNode("Реквизиты",  IconCollections.MDObjects["AttributesCollection"], Attributes));
                    m_StaticChildren.Add(new StaticTreeNode("Табличные части",  IconCollections.MDObjects["TablesCollection"], Tables));
                    m_StaticChildren.Add(new StaticTreeNode("Формы",  IconCollections.MDObjects["FormsCollection"], Forms));
                    m_StaticChildren.Add(new StaticTreeNode("Макеты",  IconCollections.MDObjects["TemplatesCollection"], Templates));

                }
                
                return m_StaticChildren;
            }

        }

        #endregion

        #region ICommandProvider
        
        public IEnumerable<UICommand> Commands
        {
            get
            {
                List<UICommand> cmdList = new List<UICommand>();

                cmdList.Add(new UICommand("Открыть модуль объекта", this, new Action(() =>
                {
                    var modProc = Properties["Module"].Value as V8ModuleProcessor;

                    modProc.GetEditor().Edit();

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

        #region IEditable implementation

        public ICustomEditor GetEditor()
        {
            return new Editors.DataProcEditor(this);
        }

        #endregion

        #region IMDPropertyProvider Members

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            var internalProps = base.PropHolder;
                
            internalProps.Add(PropDef.Create("Module", "Модуль объекта", 
                new V8ModuleProcessor(ObjectModule, "Модуль объекта:" + Name)));

            internalProps.Add(PropDef.Create("Help", "Справочная информация", Help));
            
            //internalProps.Add(PropDef.Create("Attributes", "Реквизиты", Attributes));
            //internalProps.Add(PropDef.Create("Tables", "Табличные части", Tables));
            //internalProps.Add(PropDef.Create("Forms", "Формы", Forms));
            //internalProps.Add(PropDef.Create("Templates", "Макеты", Templates));
            
        }

        #endregion

    }

}
