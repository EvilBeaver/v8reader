using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    internal class DCSSchemaDocument : TemplateDocument
    {
        public DCSSchemaDocument(MDTemplate OwnerTemplate, MDReader Reader)
            : base(OwnerTemplate, Reader)
        {
        }

        private bool _isLoaded;
        private string _SchemaContent;
        private List<String> _SettingsList;

        public string SchemaContent 
        { 
            get
            {
                LoadDataIfNeeded();
                return _SchemaContent;
            }
        }

        public IEnumerable<String> Variants
        {
            get
            {
                LoadDataIfNeeded();
                return _SettingsList.AsReadOnly();
            }
        }

        private void LoadDataIfNeeded()
        {
            if (!_isLoaded)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            MDFileItem FileElement;
            try
            {
                FileElement = Reader.GetElement(Owner.ID + ".0");
            }
            catch(System.IO.FileNotFoundException exc)
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString(), exc);
            }

            if (FileElement.ElemType == MDFileItem.ElementType.File)
            {
                using (var src = FileElement.GetStream())
                {
                    src.Seek(4, System.IO.SeekOrigin.Begin);

                    using (var rdr = new System.IO.BinaryReader(src))
                    {
                        int variantNum = rdr.ReadInt32();
                        Int64 SchemaLen = rdr.ReadInt64();

                        Int64[] lenArray = new Int64[variantNum];
                        for (int i = 0; i < variantNum; i++)
                        {
                            lenArray[i] = rdr.ReadInt64();
                        }

                        // вряд ли кто-то засунет в схему данные не влезающие в Int32
                        // поэтому, не будем заморачиваться с длиной в Int64 
                        // (BinaryReader.ReadBytes не работает c Int64)
                        _SchemaContent = Encoding.UTF8.GetString(rdr.ReadBytes((Int32)SchemaLen));
                        _SettingsList = new List<string>();

                        for (int i = 0; i < variantNum; i++)
                        {
                            _SettingsList.Add(Encoding.UTF8.GetString(rdr.ReadBytes((Int32)lenArray[i])));
                        }
                    }
                }
            }
            else
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString());
            }

        }

        public override bool CompareTo(object Comparand)
        {
            throw new NotImplementedException();
        }

        public override Comparison.IDiffViewer GetDifferenceViewer(object Comparand)
        {
            throw new NotImplementedException();
        }
    }
}
