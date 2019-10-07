using DynamicData;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HideezClient.ViewModels
{
    class AccountInfoViewModel
    {
        private readonly AccountRecord accountRecord;

        public AccountInfoViewModel(AccountRecord accountRecord)
        {
            this.accountRecord = accountRecord;

            if (accountRecord.Apps != null)
            {
                AppsUrls.AddRange(AccountUtility.Split(accountRecord.Apps));
            }
            if (accountRecord.Urls != null)
            {
                AppsUrls.AddRange(AccountUtility.Split(accountRecord.Urls));
            }
        }

        public ushort Key { get { return accountRecord.Key; } }
        public string Name { get { return accountRecord.Name; } }
        public string Login { get { return accountRecord.Login; } }
        public bool HasOpt { get { return accountRecord.HasOtp; } }
        public ObservableCollection<string> AppsUrls { get; } = new ObservableCollection<string>();
    }
}
