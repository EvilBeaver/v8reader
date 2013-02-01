using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    abstract class NamedObject
    {
        protected String m_Name;
        protected String m_Synonym;

        public NamedObject()
        {
            Name = "";
            Synonym = "";
        }

        public NamedObject(String newName, String newSynonym)
        {
            Name = newName;
            Synonym = newSynonym;
        }

        public virtual String Name
        {
            get
            {
                return m_Name;
            }
            protected set
            {
                m_Name = value;
            }
        }

        public virtual String Synonym
        {
            get
            {
                return m_Synonym;
            }
            protected set
            {
                m_Synonym = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }
        
    }
    
    abstract class MDBaseObject : NamedObject, IMDPropertyProvider
    {

        public MDBaseObject() { }
        
        public MDBaseObject(String ObjID) {
            ID = ObjID;
        }

        public MDBaseObject(SerializedList lst)
        {
            ReadFromStream(lst);
        }
        
        public String Comment { get; protected set; }

        public String ID
        {
            get { return m_ObjectID; }
            private set { m_ObjectID = value; }
        }

        protected virtual void ReadFromStream(SerializedList StringBlock)
        {
            MDBaseObject.ReadStringsBlock(this, StringBlock);
        }

        protected static void ReadStringsBlock(MDBaseObject Obj, SerializedList StringBlock)
        {
            Obj.ID = StringBlock.Items[1].Items[2].ToString();
            Obj.Name = StringBlock.Items[2].ToString();

            if (StringBlock.Items[3].Items.Count > 1)
            {
                Obj.Synonym = StringBlock.Items[3].Items[2].ToString();
            }
            else
            {
                Obj.Synonym = "";
            }

            Obj.Comment = StringBlock.Items[4].ToString();
        }

        private String m_ObjectID;


        #region IMDPropertyProvider Members

        private PropertyHolder m_Props;

        protected PropertyHolder PropHolder
        {
            get
            {
                if (m_Props == null)
                {
                    m_Props = new PropertyHolder();
                    DeclareProperties();
                }

                return m_Props;
            }
        }

        protected virtual void DeclareProperties()
        {
            PropHolder.Add("ID", "ID", ID);
            PropHolder.Add("Name", "Имя", Name);
            PropHolder.Add("Synonym", "Синоним", Synonym);
            PropHolder.Add("Comment", "Комментарий", Comment);

        }

        virtual public IDictionary<string, PropDef> Properties
        {
            get 
            {
                return PropHolder.Properties;
            }
        }

        virtual public object GetValue(string Key)
        {
            return PropHolder.GetValue(Key);
        }


        #endregion
    }

    class MDObjectsCollection<TItem> : IEnumerable<TItem>
    {

        public MDObjectsCollection()
        {
            m_Collection = new List<TItem>();
        }
        
        public TItem this[int index]
        {
            get { return m_Collection[index]; }
        }

        public void Add(TItem item)
        {
            m_Collection.Add(item);
        }

        public void RemoveAt(int index)
        {
            m_Collection.RemoveAt(index);
        }

        public void Clear()
        {
            m_Collection.Clear();
        }

        public int Count
        {
            get { return m_Collection.Count; }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return new MDObjectsEnumerator<TItem>(m_Collection);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)GetEnumerator();
        }

        private List<TItem> m_Collection;

    }

    class MDObjectsEnumerator<T> : IEnumerator<T>
    {
        private List<T> m_collection;
        private int m_CurrentIndex;
        private T m_current;

        public MDObjectsEnumerator(List<T> Collection)
        {
            m_collection = Collection;
            Reset();
        }

        public T Current
        {
            get { return m_current; }
        }

        public void Dispose()
        {
            
        }

        object System.Collections.IEnumerator.Current
        {
            get { return m_current; }
        }

        public bool MoveNext()
        {
            if (++m_CurrentIndex >= m_collection.Count)
            {
                return false;
            }
            else
            {
                m_current = m_collection[m_CurrentIndex];
                return true;
            }
        }

        public void Reset()
        {
            m_CurrentIndex = -1;
            m_current = default(T);
        }
    }

    class MDAttribute : MDBaseObject, IMDTreeItem, Comparison.IComparableItem
    {
        public MDAttribute(SerializedList attrDestription)
        {
            m_RawContent = attrDestription;

            var test = attrDestription.Items[0].Items[1].Items[0].ToString();
            SerializedList StringsBlock;
            
            if (test == "2")
            {
                StringsBlock = attrDestription.DrillDown(3);
            }
            else
            {
                StringsBlock = attrDestription.DrillDown(4);
            }
            
            MDBaseObject.ReadStringsBlock(this, StringsBlock);
        }

        private SerializedList m_RawContent;

        #region MDTreeItem

        public String Key
        {
            get { return ID; }
        }

        public String Text
        {
            get { return Name; }
        }

        public AbstractImage Icon
        {
            get {return IconCollections.MDObjects["Attribute"];}
        }

        public bool HasChildren()
        {
            return false;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get { return null; }
        }


        #endregion

        #region IComparableItem Members

        public bool CompareTo(object Comparand)
        {
            var Attrib = Comparand as MDAttribute;
            if (Attrib == null)
                return false;

            return m_RawContent.ToString() == Attrib.m_RawContent.ToString();

        }

        public Comparison.DiffViewer GetDifferenceViewer(object Comparand)
        {
            return null;
        }

        #endregion
    }

    class MDTable : MDBaseObject, IMDTreeItem
    {
        public MDTable(SerializedList tableDescription)
        {
            m_RawContent = tableDescription;
            var StringsBlock = ((SerializedList)tableDescription).DrillDown(4);
            MDBaseObject.ReadStringsBlock(this,StringsBlock);

            m_Attributes = new MDObjectsCollection<MDAttribute>();

            if (tableDescription.Items.Count > 2)
            {
                var AttributeList = (SerializedList)tableDescription.Items[2];

                FillAttributeCollection(AttributeList);

            }

        }

        public MDObjectsCollection<MDAttribute> Attributes
        {
            get { return m_Attributes; }
        }
        
        private void FillAttributeCollection(SerializedList AttrList)
        {
            for (int i = 2; i < AttrList.Items.Count(); ++i)
            {
                Attributes.Add(new MDAttribute((SerializedList)AttrList.Items[i]));
            }
        }

        private SerializedList m_RawContent;
        private MDObjectsCollection<MDAttribute> m_Attributes;

        #region MDTreeItem

        public String Key
        {
            get { return ID; }
        }

        public String Text
        {
            get { return Name; }
        }

        public AbstractImage Icon
        {
            get { return  IconCollections.MDObjects["Table"]; }
        }

        public bool HasChildren()
        {
            return true;
        }

        public IEnumerable<IMDTreeItem> ChildItems
        {
            get { return Attributes; }
        }

        #endregion

        protected override void DeclareProperties()
        {
            base.DeclareProperties();
            base.PropHolder.Add("Attributes", "Реквизиты", Attributes);
        }

    }

#region Exceptions

    class MDStreamFormatException : Exception
    {
        public MDStreamFormatException() : base("Stream format error") { }
    }

    class MDObjectIsEmpty : Exception
    {
        public MDObjectIsEmpty(String objName)
        {

        }

        public MDObjectIsEmpty(String objName, Exception InnerException)
            : base(String.Format("Object {0} is empty", objName), InnerException)
        {

        }
    }

#endregion

}
