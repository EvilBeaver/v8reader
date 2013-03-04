using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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

        public override Editors.ICustomEditor GetEditor()
        {
            XDocument result = new XDocument();
            XDocument schema = XDocument.Parse(SchemaContent);
            
            result.Add(schema.Root.Elements().First<XElement>());
            
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
                        
                        _SchemaContent = ReadUTF8Array(rdr.ReadBytes((Int32)SchemaLen));
                        _SettingsList = new List<string>();

                        for (int i = 0; i < variantNum; i++)
                        {
                            _SettingsList.Add(ReadUTF8Array(rdr.ReadBytes((Int32)lenArray[i])));
                        }
                    }
                }
            }
            else
            {
                throw new MDObjectIsEmpty(Owner.Kind.ToString());
            }

        }

        private string ReadUTF8Array(byte[] arr)
        {
#if !AS_IS
            var enc = Encoding.UTF8;
            var BOM = enc.GetPreamble();
            bool hasBOM = true;
            for (int i = 0; i < BOM.Length; i++)
            {
                if (arr[i] != BOM[i])
                {
                    hasBOM = false;
                    break;
                }
            }

            int startPoint = hasBOM ? BOM.Length : 0;
#else
            int startPoint = 0;
#endif
            return enc.GetString(arr, startPoint, arr.Length - startPoint);

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
