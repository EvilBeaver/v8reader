using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    interface IValueVisualizer
    {
        void ShowValue();

        string StringContent();
        
    }
}
