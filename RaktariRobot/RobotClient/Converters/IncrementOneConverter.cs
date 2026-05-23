using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace RuntimeTerror.Client
{
    public class IncrementOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue + 1;
            }
            if (value is bool boolValue)
            {
                return boolValue ? 2 : 1;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue - 1;
            }
            if (value is bool boolValue)
            {
                return boolValue ? 0 : -1;
            }
            return value;
        }
    }
}
