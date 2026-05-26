using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace RuntimeTerror.Client
{
    public class IncrementOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case int intValue:
                    return intValue + 1;
                case bool boolValue:
                    return boolValue ? 2 : 1;
                case double doubleValue:
                    return doubleValue + 1.0;
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case int intValue:
                    return intValue - 1;
                case bool boolValue:
                    return boolValue ? 0 : -1;
                case double doubleValue:
                    return doubleValue - 1.0;
                default:
                    return value;
            }
        }
    }
}
