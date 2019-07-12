using System;
using System.Diagnostics;

namespace HideezMiddleware.Utils
{
    public class UrlUtils
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
    }
}
