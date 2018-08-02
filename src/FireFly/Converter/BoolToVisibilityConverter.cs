using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace FireFly.Converter
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = false;
            if (parameter != null && parameter is string && (parameter as string).Equals("inverse"))
            {
                inverse = true;
            }
            if (value is bool)
            {
                return (((bool)value) ^ inverse ? Visibility.Visible : Visibility.Collapsed);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
