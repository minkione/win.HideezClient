using System;
using System.Text.RegularExpressions;

namespace HideezClient.Utilities
{
    static class URLHelper
    {
        private static IPublicSuffix publicSuffix;
        private static IPublicSuffix PublicSuffix
        {
            get
            {
                if(publicSuffix == null)
                {
                    publicSuffix = new PublicSuffix();
                }
                return publicSuffix;
            }
        }

        public static string GetRegistrableDomain(string hostname)
        {
            string tld = PublicSuffix.GetTLD(hostname);
            string registrableDomain = Regex.Match(hostname, $@"^(.+\.)?(?<registrableDomain>.+\.{tld})$", RegexOptions.IgnoreCase).Groups["registrableDomain"].Value;
            return registrableDomain;
        }

        public static bool IsUrl(string url)
        {
            string pattern = "^(((https?)|(ftp)|(file))://)?([\\da-z\\.-]+)\\.([a-z\\.]{2,6})/?"; // Url regex pattern
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute) || r.IsMatch(url))
                return true;
            else
                return false;
        }
    }
}
