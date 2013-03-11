using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class V8MetadataContainer
    {
        public V8MetadataContainer(string FileName)
        {
            _fileName = FileName;

            try
            {
                _reader = new MDReader(_fileName);
            }
            catch
            {
                _reader.Dispose();
                throw;
            }
        }

        public string FileName 
        { 
            get { return _fileName; } 
        }

        public MDObjectBase RaiseObject()
        {

            SerializedList procData = GetMainStream(_reader);

            MDDataProcessor NewMDObject = new MDDataProcessor(_reader);

            return NewMDObject;

        }

        private SerializedList GetMainStream(MDReader Reader)
        {
            var Root = new SerializedList(Reader.GetElement("root").ReadAll());
            var TOCElement = Reader.GetElement(Root.Items[1].ToString());
            return new SerializedList(TOCElement.ReadAll());
        }

        private string _fileName;
        private MDReader _reader;
    }
}
