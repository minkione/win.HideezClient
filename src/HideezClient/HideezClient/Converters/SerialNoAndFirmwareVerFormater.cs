using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace HideezClient.Converters
{
    class SerialNoAndFirmwareVerFormater : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() != 2)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            try
            {
                string serialNo = values[0]?.ToString();
                string firmwareVersion = values[1]?.ToString();

                if (!string.IsNullOrWhiteSpace(serialNo))
                {
                    sb.Append(serialNo);
                }

                if (!string.IsNullOrWhiteSpace(firmwareVersion))
                {
                    if(sb.Length > 0)
                    {
                        sb.Append(' ');
                    }

                    sb.Append('(');
                    sb.Append(firmwareVersion);
                    sb.Append(')');
                }

            }
            catch { }

            return sb.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
