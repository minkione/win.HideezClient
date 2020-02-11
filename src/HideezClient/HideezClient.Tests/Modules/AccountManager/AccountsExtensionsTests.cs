using Hideez.ARM;
using HideezClient.Modules;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace HideezClient.Tests.Modules.AccountManager
{
    [Parallelizable(ParallelScope.All)]
    public class AccountsExtensionsTests
    {
        // Saved url with domain includes domain and all subdomains
        // Saved url with subdomain includes only that subdomain
        // Port must always match between two

        [Test]
        [TestCase(true, "google.com", "google.com")]
        [TestCase(true, "account.google.com", "google.com")]
        [TestCase(true, "account.google.com", "account.google.com")]
        [TestCase(true, "office.com:80", "office.com:80")]
        [TestCase(true, "microsoft.office.com:80", "microsoft.office.com:80")]
        [TestCase(true, "microsoft.office.com:80", "office.com:80")]
        [TestCase(false, "google.com", "google.net")] // Different domains
        [TestCase(false, "google.com", "special.google.com")] // Target is for domain, saved is for subdomain
        [TestCase(false, "account.google.com", "special.google.com")] // Different subdomains
        // Cases with domain ports        
        [TestCase(false, "office.com:80", "office.com")] // Different ports
        [TestCase(false, "office.com", "office.com:80")] // Different ports
        [TestCase(false, "office.com:8", "office.com:80")] // Different ports
        [TestCase(false, "office.com:80", "office.com:8")] // Different ports
        [TestCase(false, "office.com:8080", "office.com:80")] // Different ports, similar symbols in port
        [TestCase(false, "office.com:90", "office.com:80")] // Different ports
        [TestCase(false, "microsoft.office.com:80", "google.office.com:80")] // Different subdomains
        [TestCase(false, "office.com:80", "microsoft.office.com:80")] // Target is for domain, saved is for subdomain
        [TestCase(false, "microsoft.office.com:8080", "microsoft.office.com:80")] // Different ports, similar symbols in ports
        [TestCase(false, "microsoft.office.com", "microsoft.office.com:80")] // Different ports
        [TestCase(false, "microsoft.office.com:80", "microsoft.office.com")] // Different ports
        public void MatchByDomainTest(bool expectedResult, string targetUrl, params string[] savedUrls)
        {
            // Arrange
            var appInfo = new AppInfo
            {
                Domain = targetUrl,
            };


            // Act
            var result = AccountExtensions.MatchByDomain(appInfo, savedUrls.ToList());

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        [TestCase(true, "skype", "skype")]
        [TestCase(true, "skype", "outlook", "skype")]
        [TestCase(false, "skype", "outlook")]
        [TestCase(false, "skype", "skypeoutlook")]
        [TestCase(false, "skype", "outlookskype")]
        [TestCase(false, "skype", "skypeoutlookskype")]
        [TestCase(false, "skype", "outlookskypeoutlook")]
        public void MatchByAppTest(bool expectedResult, string targetApp, params string[] savedApps)
        {
            // Arrange
            var appInfo = new AppInfo
            {
                ProcessName = targetApp,
            };


            // Act
            var result = AccountExtensions.MatchByApp(appInfo, savedApps.ToList());

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}
