using GalaSoft.MvvmLight;
using HideezSafe.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Controls
{
    public class AccountSelectorViewModel : ObservableObject
    {
        private AccountViewModel selectedAccount;

        public AccountSelectorViewModel(Account[] accounts)
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
