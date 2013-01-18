using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace V8Reader.Comparison
{
    
    [ValueConversion(typeof(ComparisonItem), typeof(Visibility))]
    class ItemBackgroundVisibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ComparisonItem Item = value as ComparisonItem;
            Controls.TreeListView tw = parameter as Controls.TreeListView;

            if (Item == null || tw == null)
            {
                return null;
            }

            if (tw.SelectedItem == Item)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
