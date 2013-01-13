using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using V8Reader.Editors;

namespace V8Reader.Core
{
    class BinaryDataDocument : TemplateDocument
    {
        public BinaryDataDocument(MDTemplate OwnerTemplate, MDReader Reader) : base(OwnerTemplate, Reader)
        {           
        }

        public System.IO.Stream GetStream()
        {
            MDFileItem Container = Reader.GetElement(GetFileName());
            SerializedList lst = new SerializedList(Container.ReadAll());

            var Base64 = lst.Items[1].Items[0].ToString();

            StringBuilder reader = new StringBuilder(Base64);
            reader.Remove(0, 8);
            Byte[] byteArr = System.Convert.FromBase64String(reader.ToString());

            MemoryStream MemStream = new MemoryStream(byteArr);

            return MemStream;

        }

        private String GetFileName()
        {
            return Owner.ID + ".0";
        }

        public override ICustomEditor GetEditor()
        {
            return new Editors.BinaryTemplateEditor(this);
        }

    }
}
