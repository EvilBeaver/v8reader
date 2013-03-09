using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace V8Reader.Core
{
    interface IValueVisualizer
    {
        Block FlowContent { get; }
        string StringContent { get; }
    }

    class SimpleTextVisualizer : IValueVisualizer
    {
        public SimpleTextVisualizer(object CurrentObject)
        {
            _currentObject = CurrentObject;
        }

        private object _currentObject;

        #region IValueVisualizer Members

        public Block FlowContent
        {
            get 
            {
                Paragraph p = new Paragraph();
                p.Margin = new System.Windows.Thickness(0);
                p.Inlines.Add(new Run(StringContent));
                return p;
            }
        }

        public string StringContent
        {
            get { return _currentObject.ToString(); }
        }

        #endregion
    }

}
