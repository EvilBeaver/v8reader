using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    partial class MDDataProcessor
    {
        public static MDDataProcessor Create(V8MetadataContainer Container, SerializedList Content)
        {

            MDDataProcessor NewMDObject = new MDDataProcessor(Content);

            return NewMDObject;
            
            
        }

    }
}
