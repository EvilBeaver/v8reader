using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    interface IMDPropertyProvider
    {
        IDictionary<string, PropDef> Properties { get; }
        
        object GetValue(string Key);

    }

    struct PropDef
    {
        public string Key;
        public string Name;
        public object Value;

        public static PropDef Create(string key, string name, object value)
        {
            return new PropDef() { Key = key, Name = name, Value = value };
        }
    }

    class PropertyHolder : IMDPropertyProvider
    {

        public PropertyHolder()
        {
            m_Props = new Dictionary<string, PropDef>();
        }

        public PropertyHolder(IDictionary<string, PropDef> PropsToHold)
        {
            m_Props = (Dictionary<string,PropDef>)PropsToHold;
        }

        public void Add(PropDef PropertyDefinition)
        {
            m_Props.Add(PropertyDefinition.Key, PropertyDefinition);
        }

        public void Add(string key, string name, object value)
        {
            PropDef prop = PropDef.Create(key, name, value);
            Add(prop);
        }

        #region IMDPropertyProvider Members

        public IDictionary<string, PropDef> Properties
        {
            get 
            {
                return m_Props;
            }
        }

        public object GetValue(string Key)
        {
            var prop = m_Props[Key];
            return prop.Value;
        }

        #endregion

        private Dictionary<string, PropDef> m_Props;

    }
}
