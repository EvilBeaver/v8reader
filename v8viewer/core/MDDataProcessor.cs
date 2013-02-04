using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    partial class MDDataProcessor : MDBaseObject, IMDTreeItem, IDisposable, IEditable, ICommandProvider
    {

        private MDDataProcessor(MDReader Reader) : base()
        {

            m_Attributes = new MDObjectsCollection<MDAttribute>();
            m_Tables     = new MDObjectsCollection<MDTable>();
            m_Forms      = new MDObjectsCollection<MDForm>();
            m_Templates  = new MDObjectsCollection<MDTemplate>();
            
            m_Reader = Reader;

            var ProcData = GetMainStream(Reader);

            ReadFromStream(ProcData);

        }

        public MDObjectsCollection<MDAttribute> Attributes 
        {
            get
            {
                CheckDisposed();
                return m_Attributes;
            }
        }
        public MDObjectsCollection<MDTable> Tables
        {
            get
            {
                CheckDisposed();
                return m_Tables;
            }
        }
        public MDObjectsCollection<MDForm> Forms
        {
            get
            {
                CheckDisposed();
                return m_Forms;
            }
        }
        public MDObjectsCollection<MDTemplate> Templates
        {
            get
            {
                CheckDisposed();
                return m_Templates;
            }
        }
        
        public HTMLDocument Help
        {
            get
            {
                CheckDisposed();
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

        public String ObjectModule
        {
            get
            {
                CheckDisposed();
                MDFileItem DirElem;

                try
                {
                    DirElem = m_Reader.GetElement(this.ID + ".0");
                }
                catch (System.IO.FileNotFoundException)
                {
                    return String.Empty; // Модуля нет
                }

                if (DirElem.ElemType == MDFileItem.ElementType.Directory)
                {
                    var textElem = DirElem.GetElement("text");

                    return textElem.ReadAll();
                }
                else
                {
                    return DirElem.ReadAll(); // если модуль зашифрован, то будет нечитаемый текст
                }
            }
        }

        public void Dispose()
        {
            m_Reader.Dispose();
        }

        ~MDDataProcessor()
        {
            m_Reader.Dispose();
        }

        #region "Private members"

        private MDReader m_Reader;
        private MDObjectsCollection<MDAttribute> m_Attributes;
        private MDObjectsCollection<MDTable> m_Tables;
        private MDObjectsCollection<MDForm> m_Forms;
        private MDObjectsCollection<MDTemplate> m_Templates;
        private HTMLDocument m_Help;

        protected override void ReadFromStream(SerializedList ProcData)
        {

            SerializedList Content = ProcData.DrillDown(3);
            
            base.ReadFromStream(Content.DrillDown(3));

            const int start = 3;
            int ChildCount = Int32.Parse(Content.Items[2].ToString());

            for (int i = 0; i < ChildCount; ++i)
            {
                SerializedList Collection = (SerializedList)Content.Items[start + i];

                String CollectionID = Collection.Items[0].ToString();
                int ItemsCount = Int32.Parse(Collection.Items[1].ToString());

                for (int itemIndex = 2; itemIndex < (2+ItemsCount); ++itemIndex)
                {
                    switch (CollectionID)
                    {
                        case MDConstants.AttributeCollection:
                            Attributes.Add(new MDAttribute((SerializedList)Collection.Items[itemIndex]));
                            break;
                        case MDConstants.TablesCollection:
                            Tables.Add(new MDTable((SerializedList)Collection.Items[itemIndex]));
                            break;
                        case MDConstants.FormCollection:
                            Forms.Add(MDForm.CreateByID(Collection.Items[itemIndex].ToString(), m_Reader));
                            break;
                        case MDConstants.TemplatesCollection:
                            Templates.Add(new MDTemplate(Collection.Items[itemIndex].ToString(), m_Reader));
                            break;
                    }
                }

            }


        }

        private void CheckDisposed()
        {
            if (m_Reader.IsDisposed())
            {
                throw new ObjectDisposedException(Name);
            }
        }


        #endregion

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
