using GalaSoft.MvvmLight;
using HideezClient.Models;
using HideezClient.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Controls
{
    public class AccountSelectorViewModel : ObservableObject
    {
        private AccountViewModel selectedAccount;

        public AccountSelectorViewModel(AccountModel[] accounts)
        {
            Accounts = accounts.Select(a => new AccountViewModel(a)).ToList();
            SelectedAccount = Accounts.FirstOrDefault();
        }

        public List<AccountViewModel> Accounts { get; }

        public AccountViewModel SelectedAccount
        {
            get { return selectedAccount; }
            set { Set(ref selectedAccount, value); }
        }
    }
}
