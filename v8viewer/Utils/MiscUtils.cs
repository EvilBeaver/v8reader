using System;
using System.Windows;
using System.Windows.Media;

namespace V8Reader.Utils
{    
    static class UIHelper
    {
        public static void DefaultErrHandling(Exception exc)
        {
            MessageBox.Show(exc.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        public static T FindLogicalParent<T>(DependencyObject childElement) where T : DependencyObject
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(childElement);
            T parentAsT = parent as T;
            if (parent == null)
            {
                return null;
            }
            else if (parentAsT != null)
            {
                return parentAsT;
            }
            return FindLogicalParent<T>(parent);
        }

        public static T FindVisualParent<T>(DependencyObject childElement) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(childElement);
            T parentAsT = parent as T;
            if (parent == null)
            {
                return null;
            }
            else if (parentAsT != null)
            {
                return parentAsT;
            }
            return FindVisualParent<T>(parent);
        }

    }
    


}