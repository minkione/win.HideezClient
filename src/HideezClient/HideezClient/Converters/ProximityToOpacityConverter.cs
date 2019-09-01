using System;
using System.Globalization;
using System.Windows.Data;

namespace HideezClient.Converters
{
    public class ProximityToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double proximity = (double)value;
            int limit = System.Convert.ToInt32(parameter);
            if (proximity > limit)
                return 1.0;
            else
                return 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
