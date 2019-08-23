using HideezSafe.Models;
using HideezSafe.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Controls
{
    public class AccountViewModel
    {
        public AccountViewModel(Account account)
        {
            Account = account;
        }

        public string FullName { get { return Account.Name; } }

        public Account Account { get; }
    }
}
