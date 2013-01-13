using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class MDTemplate : MDBaseObject, IMDTreeItem, IEditable
    {
        public enum TemplateKind
        {
            Moxel = 0,
            Text = 4,
            BinaryData = 1,
            ActiveDocument = 2,
            HTMLDocument = 3,
            GEOSchema = 5,
            GraphicChart = 8,
            DataCompositionSchema = 6,
            DCSAppearanceTemplate = 7
        }

        public MDTemplate(String ObjID, MDReader Reader)
        {
            m_Reader = Reader;

            SerializedList header = new SerializedList(Reader.GetElement(ObjID).ReadAll());
            Kind = (TemplateKind)Enum.Parse(typeof(TemplateKind), header.Items[1].Items[1].ToString());

            base.ReadFromStream((SerializedList)header.Items[1].Items[2]);

            switch (Kind)
            {
                case MDTemplate.TemplateKind.Moxel:
                case MDTemplate.TemplateKind.Text:
                case MDTemplate.TemplateKind.GEOSchema:
                case MDTemplate.TemplateKind.GraphicChart:
                //case MDTemplate.TemplateKind.DataCompositionSchema:
                case MDTemplate.TemplateKind.DCSAppearanceTemplate:
                    m_Document = new PersistedTemplateStub(this, Reader);
                    break;
                case MDTemplate.TemplateKind.BinaryData:
                    m_Document = new BinaryDataDocument(this, Reader);
                    break;
                case MDTemplate.TemplateKind.HTMLDocument:
                    m_Document = new HTMLTemplate(this, Reader);
                    break;
                default:
                    break;
            }

        }

        public TemplateKind Kind { get; protected set; }

        protected MDReader m_Reader;

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
            get { return  IconCollections.MDObjects["Template"]; }
        }

        public virtual bool HasChildren()
        {
            return false;
        }

        public virtual IEnumerable<IMDTreeItem> ChildItems
        {
            get { return null; }
        }

        protected TemplateDocument m_Document;


        public ICustomEditor GetEditor()
        {
            if (m_Document != null)
                return m_Document.GetEditor();
            else
                throw new NotSupportedException("Редактирование макета данного типа не поддерживается");
        }
    }
}
