using Hideez.ARM;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Models;
using HideezClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    public static class AccountExtensions
    {
        public static Account[] FindAccountsByApp(this Device device, AppInfo appInfo)
        {
            List<Account> accounts = new List<Account>();

            foreach (var accountRecord in device.PasswordManager?.Accounts)
            {
                // login must be the same
                if (!string.IsNullOrEmpty(appInfo.Login))
                {
                    if (!appInfo.Login.Equals(accountRecord.Login, StringComparison.OrdinalIgnoreCase))
                        break;
                }

                if (MatchByDomain(appInfo, AccountUtility.Split(accountRecord.Urls))
                    || MatchByApp(appInfo, AccountUtility.Split(accountRecord.Apps)))
                {
                    accounts.Add(new Account(device, accountRecord));
                }
            }

            return accounts.ToArray();
        }

        // Marked as internal for unit tests
        internal static bool MatchByDomain(AppInfo appInfo, IEnumerable<string> domains)
        {
            return !string.IsNullOrWhiteSpace(appInfo.Domain)
                && domains.FirstOrDefault(d => appInfo.MatchesDomain(d)) != null;
        }

        internal static bool MatchByApp(AppInfo appInfo, IEnumerable<string> apps)
        {
            foreach (var app in apps)
            {
                if (!string.IsNullOrWhiteSpace(appInfo.Description))
                {
                    if (app.Contains('=')) // Description in format [App Name=bundle.id]
                    {
                        if (app.IndexOf(appInfo.Description, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                    else
                    {
                        if (appInfo.Description.Equals(app, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }

                if (!string.IsNullOrWhiteSpace(appInfo.ProcessName))
                {
                    if (appInfo.ProcessName.Equals(app, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}
