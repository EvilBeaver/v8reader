using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    partial class MDDataProcessor
    {
        public static MDDataProcessor Create(String ImageFile)
        {

            MDReader Reader = null;

            try
            {
                Reader = new MDReader(ImageFile);
            }
            catch
            {
                if (Reader != null)
                    Reader.Dispose();
                throw;
            }

            SerializedList procData = GetMainStream(Reader);

            MDDataProcessor NewMDObject = new MDDataProcessor(Reader);

            return NewMDObject;
            
            
        }

        private static SerializedList GetMainStream(MDReader Reader)
        {
            var Root = new SerializedList(Reader.GetElement("root").ReadAll());
            var TOCElement = Reader.GetElement(Root.Items[1].ToString());
            return new SerializedList(TOCElement.ReadAll());
        }
    }
}
