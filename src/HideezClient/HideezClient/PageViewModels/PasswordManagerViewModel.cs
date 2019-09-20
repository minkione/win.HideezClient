using HideezClient.Models;
using HideezClient.Mvvm;
using HideezClient.Utilities;
using HideezClient.ViewModels;
using MvvmExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace HideezClient.PageViewModels
{
    class PasswordManagerViewModel : LocalizedObject
    {
        private DelayedAction delayedFilteredPasswordsRefresh;
        private DeviceViewModel device;
        private AccountViewModel selectedAccount;
        private string searchQuery;

        private readonly List<AccountViewModel> allAccounts = new List<AccountViewModel>
        {
            new AccountViewModel { Name = "Pizza Hut", Login = "john.gardner@example.com", HasOpt = true, },
            new AccountViewModel { Name = "The Walt Disney Company", Login = "seth.olson@example.com", },
            new AccountViewModel { Name = "Bank of America", Login = "penny.nichols@example.com", },
            new AccountViewModel { Name = "eBay", Login = "alice.bryant@example.com", },
            new AccountViewModel { Name = "MasterCard", Login = "tamara.kuhn@example.com", },
            new AccountViewModel { Name = "Johnson & Johnson", Login = "keith.richards@example.com", HasOpt = true, },
            new AccountViewModel { Name = "Starbucks", Login = "logan.hopkins@example.com", },
            new AccountViewModel { Name = "Facebook", Login = "kelly.howard@example.com", },
            new AccountViewModel { Name = "L'Oréal", Login = "jeff.anderson@example.com", },
            new AccountViewModel { Name = "Mitsubishi", Login = "dan.romero@example.com", HasOpt = true, },
            new AccountViewModel { Name = "Apple", Login = "gary.herrera@example.com", },
            new AccountViewModel { Name = "Louis Vuitton", Login = "jessica.hanson@example.com", },
        };

        public PasswordManagerViewModel()
        {
            try
            {
                delayedFilteredPasswordsRefresh = new DelayedAction(() =>
                {
                    Application.Current.Dispatcher.Invoke(() => { Accounts.Refresh(); });
                }, 100);
            }
            catch (Exception ex)
            {
            }
        }

        public DeviceViewModel Device
        {
            get { return device; }
            set { Set(ref device, value); }
        }


        public AccountViewModel SelectedAccount
        {
            get { return selectedAccount; }
            set { Set(ref selectedAccount, value); }
        }


        public ICollectionView Accounts
        {
            get
            {
                var view = CollectionViewSource.GetDefaultView(allAccounts);
                view.Filter = Filter;

                var enumerator = view.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    SelectedAccount = enumerator.Current as AccountViewModel;
                }
                
                return view;
            }
        }

        public string SearchQuery
        {
            get { return searchQuery; }
            set
            {
                if (searchQuery != value)
                {
                    Set(ref searchQuery, value);
                    delayedFilteredPasswordsRefresh.RunDelayedAction();
                }
            }
        }

        private bool Filter(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return true;

            var account = item as AccountViewModel;
            var trimmedSearchString = SearchQuery.Trim();

            // Check search string match for each parameter
            bool nameMatch = account.Name?.IndexOf(trimmedSearchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool loginMatch = account.Login?.IndexOf(trimmedSearchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool keywordsMatch = (account.WebSiteApp != null) 
                ? string.Join(",", account.WebSiteApp).IndexOf(trimmedSearchString, StringComparison.InvariantCultureIgnoreCase) >= 0 
                : false;

            // Filter by name, keywords or login
            return nameMatch || loginMatch || keywordsMatch;
        }
    }

    class AccountViewModel : LocalizedObject
    {
        private string name;
        private string login;
        private bool hasOpt;

        private ObservableCollection<string> webSiteApp = new ObservableCollection<string>()
        {
            "facebook.com",
            "google.com",
            "facebook.com",
            "facebook.com",
        };

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public string Login
        {
            get { return login; }
            set { Set(ref login, value); }
        }

        [DependsOn("HasOpt")]
        public string Otp
        {
            get { return HasOpt ? "enabled" : "disabled"; }
        }

        public bool HasOpt
        {
            get { return hasOpt; }
            set { hasOpt = value; }
        }

        public ObservableCollection<string> WebSiteApp
        {
            get { return webSiteApp; }
            set { Set(ref webSiteApp, value); }
        }
    }
}
