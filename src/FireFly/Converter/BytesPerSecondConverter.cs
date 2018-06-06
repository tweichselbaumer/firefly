using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FireFly.Converter
{
    public class BytesPerSecondConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double val = (double)value;
                if (val * 8 / 1024 / 1024 > 0.8)
                {
                    return string.Format("{0:0.00} MBit/s", val * 8 / 1024 / 1024);
                }
                else if (val * 8 / 1024 > 0.8)
                {
                    return string.Format("{0:0.00} KBit/s", val * 8 / 1024);
                }
                else
                {
                    return string.Format("{0:0.00} Bit/s", val * 8);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
