using System.Text.RegularExpressions;

namespace WinBle
{
    internal class WinBleUtils
    {
        internal static string GetMacAddress(ulong bluetoothAddress)
        {
            var tempMac = bluetoothAddress.ToString("X");

            var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
            var replace = "$1:$2:$3:$4:$5:$6";
            var macAddress = Regex.Replace(tempMac, regex, replace);
            return macAddress;
        }
    }
}
