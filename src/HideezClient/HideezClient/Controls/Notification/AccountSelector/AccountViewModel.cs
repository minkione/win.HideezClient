using HideezClient.Models;
using HideezClient.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Controls
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
