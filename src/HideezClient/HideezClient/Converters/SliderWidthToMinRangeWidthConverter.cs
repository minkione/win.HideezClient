using System;
using System.Globalization;
using System.Windows.Data;

namespace HideezClient.Converters
{
    class SliderWidthToMinRangeWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var minRange = int.Parse((string)parameter);
            var actualWidth = (double)value;

            return (actualWidth / 100) * minRange;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
