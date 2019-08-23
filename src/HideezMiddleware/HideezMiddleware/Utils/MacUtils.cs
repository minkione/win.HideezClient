using System.Linq;

namespace HideezMiddleware.Utils
{
    public static class MacUtils
    {
        public static bool AreEqual(string a_mac, string b_mac)
        {
            // Converting to short mac is faster, that converting to mac
            return ConvertMacToShortMac(a_mac) == ConvertMacToShortMac(b_mac);
        }

        public static string GetMacFromShortMac(string shortMac)
        {
            if (shortMac == null)
                return null;

            // Insert ':' between every 2 characters, unless they are already present
            if (!shortMac.Contains(":"))
                shortMac = string.Join(":", Enumerable.Range(0, 6).Select(i => shortMac.Substring(i * 2, 2)));

            return shortMac;
        }

        public static string ConvertMacToShortMac(string mac)
        {
            return mac?.Replace(":", "");
        }
    }
}
