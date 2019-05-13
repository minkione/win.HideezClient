using System;
using System.Windows;
using System.Windows.Data;

namespace HideezSafe.Converters
{
    public class StringToResource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object resource = Application.Current.FindResource(value as string);
            return resource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
