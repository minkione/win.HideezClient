using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HideezSafe.Utilities;

namespace HideezSafe.Tests
{
    [TestClass]
    public class AccountUtilityTest
    {
        [TestMethod]
        public void TestMethodTryGetDomain()
        {
            foreach (var url in CreateUrls())
            {
                Debug.WriteLine("=========================");
                Debug.WriteLine($"### Url: {url}");

                if (AccountUtility.TryGetDomain(url, out string domain))
                {
                    Debug.WriteLine($"### Domain: {domain}");

                    Assert.IsTrue(domains.Contains(domain));
                }
            }
        }

        static string[] domains;
        private static string[] CreateUrls()
        {
            domains = new[]
            {
                "domain",
                "domain.com",
                "dom;ain.com",
                "125.23.55.22",
                "subdomain.domain.com",
                "subdomain.125.23.55.22",
            };

            List<string> urls = new List<string>();

            var testDomain = new List<string>(domains);
            testDomain.AddRange(domains.Select(d => $"{d}:8080"));
            foreach (var domain in testDomain)
            {
                urls.Add(domain);
                urls.Add($"{domain}/");
                urls.Add($"{domain}/somedata");

                urls.Add($"www.{domain}");
                urls.Add($"www.{domain}/");
                urls.Add($"www.{domain}/somedata");

                urls.Add($"http://www.{domain}");
                urls.Add($"http://www.{domain}/");
                urls.Add($"http://www.{domain}/somedata");

                urls.Add($"https://www.{domain}");
                urls.Add($"https://www.{domain}/");
                urls.Add($"https://www.{domain}/somedata");

                urls.Add($"http://{domain}");
                urls.Add($"http://{domain}/");
                urls.Add($"http://{domain}/somedata");

                urls.Add($"https://{domain}");
                urls.Add($"https://{domain}/");
                urls.Add($"https://{domain}/somedata");

                urls.Add($"fttp://{domain}");
                urls.Add($"fttp://{domain}/");
                urls.Add($"fttp://{domain}/somedata");
            }
            urls.Add("file://c:/WINDOWS/clock.avi");

            return urls.ToArray();
        }
    }
}
