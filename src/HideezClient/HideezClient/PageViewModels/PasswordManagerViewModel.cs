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
using HideezClient.Extension;
using System.Windows.Input;
using MvvmExtensions.Commands;

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
            new AccountViewModel {  Login = "jeff.anderson@example.com", },
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

            if (item is AccountViewModel account)
            {
                var filter = SearchQuery.Trim();
                return Contains(account.Name, filter) || Contains(account.Login, filter) || (account.WebSiteApp?.Any(s => Contains(s, filter)) ?? false);
            }

            return false;
        }
        private bool Contains(string source, string toCheck)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        #region Command

        public ICommand AddAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnAddAccount();
                    },
                };
            }
        }

        #endregion

        private void OnAddAccount()
        {
            // TODO: Implement add account
            MessageBox.Show("Add account");
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

        #region Command

        public ICommand EditAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnEditAccount();
                    },
                };
            }
        }

        #endregion

        private void OnEditAccount()
        {
            // TODO: Implement edit account
            MessageBox.Show("Edit account");
        }
    }
}
