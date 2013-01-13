using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    
    abstract class TemplateDocument : IEditable
    {
        public TemplateDocument(MDTemplate OwnerTemplate, MDReader Reader)
        {
            m_Owner = OwnerTemplate;
            m_Reader = Reader;
        }

        public MDTemplate Owner
        {
            get
            {
                return m_Owner;
            }
            
        }

        protected MDReader Reader
        {
            get { return m_Reader; }
        }

        public virtual ICustomEditor GetEditor()
        {
            throw new NotImplementedException();
        }

        private MDReader m_Reader;
        private MDTemplate m_Owner;

    }

    // Базовый класс для макетов, открываемых с помощью "Работы с файлами" (File Workshop)
    // Основное назначение - обертка для сырых данных, передаваемых в File Workshop

    abstract class FWOpenableDocument : TemplateDocument
    {
        public FWOpenableDocument(MDTemplate OwnerTemplate, MDReader Reader) : base(OwnerTemplate, Reader) { }

        public override ICustomEditor GetEditor()
        {
            return new Editors.FileWorkshopEditor(this);
        }

        public virtual String Extract()
        {
            var FileName = GetFileName();
            MDFileItem FileElement;

            try
            {
                FileElement = Reader.GetElement(FileName);
            }
            catch (System.IO.FileNotFoundException exc)
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString(), exc);
            }            

            if (FileElement.ElemType == MDFileItem.ElementType.File)
            {

                System.IO.Stream SourceStream = null;
                System.IO.FileStream DestStream = null;
                
                String Result = System.IO.Path.GetTempPath() + FWOpenableName;
                
                try
                {   
                    SourceStream = new System.IO.FileStream(FileElement.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    DestStream = new System.IO.FileStream(Result, System.IO.FileMode.OpenOrCreate);

                    SourceStream.CopyTo(DestStream);

                }
                finally
                {
                    if (SourceStream != null) SourceStream.Close();
                    if (DestStream != null) DestStream.Close();
                }

                return Result;


            }
            else
                throw new MDObjectIsEmpty(Owner.Kind.ToString());

        }

        private String FWOpenableName
        {
            get
            {
                
                String FileExt = "";

                switch (Owner.Kind)
                {
                    case MDTemplate.TemplateKind.Moxel:
                        FileExt = ".mxl";
                        break;
                    case MDTemplate.TemplateKind.Text:
                        FileExt = ".txt";
                        break;
                    case MDTemplate.TemplateKind.GEOSchema:
                        FileExt = ".geo";
                        break;
                    case MDTemplate.TemplateKind.GraphicChart:
                        FileExt = ".grs";
                        break;
                    //case MDTemplate.TemplateKind.DataCompositionSchema:
                    //    FileExt = ".xml";
                    //    break;
                    case MDTemplate.TemplateKind.DCSAppearanceTemplate:
                        FileExt = ".txt";
                        break;
                    default:
                        throw new NotSupportedException();
                }

                return System.IO.Path.GetRandomFileName() + FileExt;

            }
        }

        protected virtual String GetFileName()
        {
            return Owner.ID + ".0";
        }

    }
}
