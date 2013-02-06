using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using V8Reader.Editors;

namespace V8Reader.Core
{
    
    abstract class TemplateDocument : IEditable, Comparison.IComparableItem
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


        #region IComparableItem Members

        public abstract bool CompareTo(object Comparand);
        public abstract Comparison.IDiffViewer GetDifferenceViewer(object Comparand);
        
        #endregion
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
            String Result = System.IO.Path.GetTempPath() + FWOpenableName;

            using (var SourceStream = GetDataStream())
            {
                using (var DestStream = new System.IO.FileStream(Result, System.IO.FileMode.OpenOrCreate))
                {
                    SourceStream.CopyTo(DestStream);
                }
            }
            
            return Result;

        }

        private System.IO.Stream GetDataStream()
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
                return FileElement.GetStream();
            }
            else
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString());
            }
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

        private bool IsEmpty()
        {
            try
            {
                var FileElement = Reader.GetElement(GetFileName());
                if (FileElement.ElemType == MDFileItem.ElementType.File)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (System.IO.FileNotFoundException)
            {
                return true;
            }
        }

        #region IComparableItem Members

        public override bool CompareTo(object Comparand)
        {
            FWOpenableDocument cmpDoc = Comparand as FWOpenableDocument;

            bool docEmpty = cmpDoc.IsEmpty();
            bool CurrentIsEmpty = this.IsEmpty();

            if (cmpDoc != null)
            {
                if (docEmpty)
                {
                    return CurrentIsEmpty;
                }
                else if (!CurrentIsEmpty)
                {
                    Comparison.StreamComparator sc = new Comparison.StreamComparator();

                    return sc.CompareStreams(GetDataStream(), cmpDoc.GetDataStream());

                }
                else
                {
                    return true;
                }
            }
            else
            {
                return CurrentIsEmpty;
            }         

        }

        public override Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            FWOpenableDocument cmpDoc = Comparand as FWOpenableDocument;

            string path1 = this.Extract();
            string path2 = cmpDoc.Extract();

            var DiffViewer = new Comparison.FWDiffViewer(path1, path2);

            return DiffViewer;

        }

        #endregion

    }
}
