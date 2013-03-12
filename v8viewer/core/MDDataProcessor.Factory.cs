using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    partial class MDDataProcessor
    {
        public static MDDataProcessor Create(IV8MetadataContainer Container, SerializedList Content)
        {

            MDDataProcessor NewMDObject = new MDDataProcessor();

            NewMDObject.Container = Container;

            ReadFromStream(NewMDObject, Content);

            return NewMDObject;
            
            
        }

        private static void ReadFromStream(MDDataProcessor NewMDObject, SerializedList ProcData)
        {
            const String AttributeCollection = "ec6bb5e5-b7a8-4d75-bec9-658107a699cf";
            const String TablesCollection = "2bcef0d1-0981-11d6-b9b8-0050bae0a95d";
            const String FormCollection = "d5b0e5ed-256d-401c-9c36-f630cafd8a62";
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

    }
}
