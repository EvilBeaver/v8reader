using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Comparison
{
    class BasicComparator : IComparator
    {

        #region IComparator Members

        public bool CompareObjects(object Compared, object Comparand)
        {
            if (Compared == null)
                return Comparand == null;

            return Compared.Equals(Comparand);
        }

        #endregion
    }

    class ToStringComparator : IComparator
    {

        private ToStringComparator()
        {
        }

        #region IComparator Members

        public bool CompareObjects(object Compared, object Comparand)
        {
            // null<>null
            if (Compared == null)
                return false;
            if (Comparand == null)
                return false;

            return Compared.ToString() == Comparand.ToString();
        }

        private static readonly ToStringComparator _comparatorObject = new ToStringComparator();

        public static ToStringComparator ComparatorObject
        {
            get
            {
                return _comparatorObject;
            }
        }

        #endregion
    }

}
