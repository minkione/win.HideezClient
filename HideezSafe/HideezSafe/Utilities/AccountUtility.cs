using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Utilities
{
    static class AccountUtility
    {
        public static bool TryGetDomain(string url, out string domain)
        {
            string uriString = url;
            domain = null;

            if (!string.IsNullOrWhiteSpace(uriString))
            {
                try
                {
                    if (!uriString.Contains(Uri.SchemeDelimiter))
                    {
                        uriString = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, uriString);
                    }
                    domain = new Uri(uriString).Host;

                    if (domain.StartsWith("www."))
                        domain = domain.Remove(0, 4);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    domain = null;
                }
            }

            return !string.IsNullOrWhiteSpace(domain);
        }

        public static string[] Split(string data)
        {
            if (string.IsNullOrEmpty(data))
                return Array.Empty<string>();

            // string[] separators = { "\r\n", "\n", "\r" };
            string[] words = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
                words[i] = words[i].Trim();

            return words;
        }
    }
}
