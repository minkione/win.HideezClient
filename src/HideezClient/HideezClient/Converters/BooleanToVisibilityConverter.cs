﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HideezClient.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = ((bool)value == true) ? Visibility.Visible : Visibility.Collapsed;
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
