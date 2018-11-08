using System;
using System.Globalization;
using System.Windows.Data;

namespace FireFly.Converter
{
    public class MultiValueReplayBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool a = (bool)values[0];
            bool b = (bool)values[1];
            bool c = (bool)values[2];
            bool d = (bool)values[3];

            if (d)
            {
                return false;
            }

            switch ((string)parameter)
            {
                case "paus":
                    return a && b && !c;

                case "play":
                    return !a || a && b && c;

                case "stop":
                    return a && b;

                default:
                    return false;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}