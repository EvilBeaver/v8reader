using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;

namespace V8Reader.Comparison
{

    class TreeIndentConverter : IValueConverter
    {
        public const double Indentation = 10;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //If the value is null, don't return anything
            if (value == null) return null;
            //Convert the item to a double
            if (targetType == typeof(double) && typeof(DependencyObject).IsAssignableFrom(value.GetType()))
            {
                //Cast the item as a DependencyObject
                DependencyObject Element = value as DependencyObject;
                //Create a level counter with value set to -1
                int Level = -1;
                //Move up the visual tree and count the number of TreeViewItem's.
                for (; Element != null; Element = VisualTreeHelper.GetParent(Element))
                    //Check whether the current elemeent is a TreeViewItem
                    if (typeof(TreeViewItem).IsAssignableFrom(Element.GetType()))
                        //Increase the level counter
                        Level++;
                //Return the indentation as a double
                return Indentation * Level;
            }
            //Type conversion is not supported
            throw new NotSupportedException(
                string.Format("Cannot convert from <{0}> to <{1}> using <TreeListViewConverter>.",
                value.GetType(), targetType));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("This method is not supported.");
        }

    }

    [ValueConversion(typeof(string), typeof(ComparisonStatus))]
    class ComparisonStatusConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            if (value.GetType() == typeof(ComparisonStatus))
            {
                return (ComparisonStatus) value;
            }

            var stringVal = value as string;
            if (stringVal == null)
                return null;

            ComparisonStatus result;
            if (System.Enum.TryParse<ComparisonStatus>(stringVal, true, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
            

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
