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

            NewMDObject.m_Container = Container;

            ReadFromStream(NewMDObject, Content);

            return NewMDObject;
            
            
        }

        private static void ReadFromStream(MDDataProcessor NewMDObject, SerializedList ProcData)
        {
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
                        case MDConstants.AttributeCollection:
                            NewMDObject.m_Attributes.Add(new MDAttribute((SerializedList)Collection.Items[itemIndex]));
                            break;
                        case MDConstants.TablesCollection:
                            NewMDObject.m_Tables.Add(new MDTable((SerializedList)Collection.Items[itemIndex]));
                            break;
                        case MDConstants.FormCollection:
                            NewMDObject.m_Forms.Add(MDForm.Create(NewMDObject.m_Container, Collection.Items[itemIndex].ToString()));
                            break;
                        case MDConstants.TemplatesCollection:
                            NewMDObject.m_Templates.Add(new MDTemplate(NewMDObject.m_Container, Collection.Items[itemIndex].ToString()));
                            break;
                    }
                }

            }
        }

    }
}
