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
        [Test]
        [TestCase(false, "google.com", "google.net")]
        [TestCase(true, "google.com", "google.com")]
        [TestCase(true, "account.google.com", "google.com")]
        [TestCase(true, "account.google.com", "account.google.com")]
        [TestCase(false, "google.com", "special.google.com")]
        [TestCase(false, "account.google.com", "special.google.com")]
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
