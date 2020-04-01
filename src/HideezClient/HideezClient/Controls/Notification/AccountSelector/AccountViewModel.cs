using HideezClient.Models;
using System.Text;

namespace HideezClient.Controls
{
    public class AccountViewModel
    {
        public AccountViewModel(AccountModel account)
        {
            Account = account;
        }

        public AccountModel Account { get; }

        public string FullName 
        { 
            get 
            {
                var sb = new StringBuilder();

                sb.Append(Account.Name.Trim());

                if (!string.IsNullOrWhiteSpace(Account.Login))
                    sb.Append($" - {Account.Login.Trim()}");

                return sb.ToString(); 
            } 
        }

        public bool IsReadOnly => Account.IsReadOnly;

    }
}
