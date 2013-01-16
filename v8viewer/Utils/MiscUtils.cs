using System;
using System.Windows;

namespace V8Reader.Utils
{    
    static class UIHelper
    {
        public static void DefaultErrHandling(Exception exc)
        {
            MessageBox.Show(exc.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }
    


}