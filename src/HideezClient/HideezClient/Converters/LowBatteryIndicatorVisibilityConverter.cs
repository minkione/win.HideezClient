using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HideezClient.Converters
{
    class LowBatteryIndicatorVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if(values[0] is bool isConnected && values[1] is bool isInitialized && values[2] is int battery)
            {
                if(isConnected && isInitialized && battery != 0 && battery <= 25)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
