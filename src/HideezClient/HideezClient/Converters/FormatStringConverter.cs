using System;
using System.Linq;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace HideezClient.Converters
{
    public class FormatStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var format = (string)values?.FirstOrDefault()?.ToString();

            if (string.IsNullOrEmpty(format))
                return string.Empty;

            var args = values?.Skip(1)?.ToArray();
            if (args == null)
                return format;

            return string.Format(format, args);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
