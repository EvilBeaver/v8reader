using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDReport : MDObjectClass, IMDTreeItem, IEditable, ICommandProvider
    {
        private MDReport() : base()
	    {

	    }

        public static MDReport Create(IV8MetadataContainer Container, SerializedList Content)
        {
            MDReport NewReport = new MDReport();
            NewReport.Container = Container;

            ReadFromStream(NewReport, Content);

            return NewReport;
        }

        private static void ReadFromStream(MDReport NewMDObject, SerializedList ProcData)
        {
            const String AttributeCollection = "7e7123e0-29e2-11d6-a3c7-0050bae0a776";
            const String TablesCollection = "b077d780-29e2-11d6-a3c7-0050bae0a776";
            const String FormCollection = "a3b368c0-29e2-11d6-a3c7-0050bae0a776";
            const String TemplatesCollection = "3daea016-69b7-4ed4-9453-127911372fe6";

            SerializedList Content = ProcData.DrillDown(3);

            NewMDObject.ReadStringsBlock(Content.DrillDown(3));

            const int start = 3;
            int ChildCount = Int32.Parse(Content.Items[2].ToString());

            for (int i = 0; i < ChildCount; ++i)
            {
                SerializedList Collection = (SerializedList)Content.Items[start + i];

                String CollectionID = Collection.Items[0].ToString();
                int ItemsCount = Int32.Parse(Collection.Items[1].ToString());

                for (int itemIndex = 2; itemIndex < (2 + ItemsCount); ++itemIndex)
                {
                    switch (CollectionID)
                    {
                        case AttributeCollection:
                            NewMDObject.Attributes.Add(new MDAttribute((SerializedList)Collection.Items[itemIndex]));
                            break;
                        case TablesCollection:
                            NewMDObject.Tables.Add(new MDTable((SerializedList)Collection.Items[itemIndex]));
                            break;
                        case FormCollection:
                            NewMDObject.Forms.Add(MDForm.Create(NewMDObject.Container, Collection.Items[itemIndex].ToString()));
                            break;
                        case TemplatesCollection:
                            NewMDObject.Templates.Add(new MDTemplate(NewMDObject.Container, Collection.Items[itemIndex].ToString()));
                            break;
                    }
                }

            }
        }

        protected override string ObjectModuleFile()
        {
            return this.ID + ".0";
        }

        protected override string HelpFile()
        {
            return this.ID + ".1";
        }

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
            get { return IconCollections.MDObjects["DataProcessor"]; }
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

                    m_StaticChildren.Add(new StaticTreeNode("Реквизиты", IconCollections.MDObjects["AttributesCollection"], Attributes));
                    m_StaticChildren.Add(new StaticTreeNode("Табличные части", IconCollections.MDObjects["TablesCollection"], Tables));
                    m_StaticChildren.Add(new StaticTreeNode("Формы", IconCollections.MDObjects["FormsCollection"], Forms));
                    m_StaticChildren.Add(new StaticTreeNode("Макеты", IconCollections.MDObjects["TemplatesCollection"], Templates));

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
            return new Editors.ReportEditor(this);
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

        }

        #endregion

    }
}
