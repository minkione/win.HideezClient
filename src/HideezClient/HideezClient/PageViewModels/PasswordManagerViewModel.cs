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
using Hideez.SDK.Communication.PasswordManager;
using System.Security;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using DynamicData;
using DynamicData.Binding;
using System.Collections.Specialized;

namespace HideezClient.PageViewModels
{
    class PasswordManagerViewModel : LocalizedObject
    {
        private DelayedAction delayedFilteredPasswordsRefresh;
        private DeviceViewModel device;
        private AccountViewModel selectedAccount;
        private string searchQuery;

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
                var view = CollectionViewSource.GetDefaultView(Device.Accounts);
                if (view != null)
                {
                    view.Filter = Filter;

                    var enumerator = view.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        SelectedAccount = enumerator.Current as AccountViewModel;
                    }
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
                return Contains(account.Name, filter) || Contains(account.Login, filter) || account.Apps.Any(a => Contains(a, filter) || account.Urls.Any(u => Contains(u, filter)));
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

    class AccountViewModel : ReactiveObject
    {
        private readonly char separator = ';';

        public AccountViewModel(AccountRecord accountRecord)
        {
            Init(accountRecord);
        }

        private void Init(AccountRecord accountRecord)
        {
            Name = accountRecord.Name;
            Login = accountRecord.Login;
            HasOpt = accountRecord.HasOtp;

            if (accountRecord.Apps != null)
            {
                foreach (var app in accountRecord.Apps.Split(separator))
                {
                    Apps.Add(app);
                }
            }
            if (accountRecord.Urls != null)
            {
                foreach (var url in accountRecord.Urls.Split(separator))
                {
                    Urls.Add(url);
                }
            }

            this.WhenAnyValue(vm => vm.Name, vm => vm.Login, vm => vm.Password, vm => vm.HasOpt, vm => vm.OtpSecret).Subscribe(_ => HasChanges = true);
            Apps.CollectionChanged += CollectionChanged;
            Urls.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HasChanges = true;
        }

        [Reactive]
        public bool HasChanges { get; set; }
        [Reactive]
        public string Name { get; set; }
        [Reactive]
        public string Login { get; set; }
        [Reactive]
        public SecureString Password { get; set; }
        [Reactive]
        public bool HasOpt { get; protected set; }
        [Reactive]
        public string OtpSecret { get; set; }

        public ObservableCollection<string> Apps { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Urls { get; } = new ObservableCollection<string>();

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

        public ICommand UpdateAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnUpdateAccount();
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

        private void OnUpdateAccount()
        {
            HasChanges = false;
        }
    }
}
