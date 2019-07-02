using Hideez.ARS;
using Hideez.SDK.Communication.PasswordManager;
using HideezSafe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    public static class AccountExtensions
    {
        public static Account[] FindAccountsByApp(this Device device, AppInfo appInfo)
        {
            List<Account> accounts = new List<Account>();

            foreach (var accountRecord in device.PasswordManager.Accounts.Values)
            {
                // login must be the same
                if (!string.IsNullOrEmpty(appInfo.Login))
                {
                    if (!appInfo.Login.Equals(accountRecord.Login, StringComparison.OrdinalIgnoreCase))
                        break;
                }

                string[] apps = Account.SplitAppsToLines(accountRecord.Apps);
                string[] urls = Account.SplitAppsToLines(accountRecord.Urls);

                if (ContainsKeywords(appInfo, apps) || ContainsKeywords(appInfo, urls))
                {
                    accounts.Add(new Account(device, accountRecord));
                }
            }

            return accounts.ToArray();
        }

        private static bool ContainsKeywords(AppInfo appInfo, string[] apps)
        {
            if (apps == null || apps.Length == 0)
                return false;

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
                if (!string.IsNullOrWhiteSpace(appInfo.Domain))
                {
                    if (appInfo.MatchesDomain(app))
                        return true;
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
