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
using System.Reactive.Linq;
using System.Windows.Controls;

namespace HideezClient.PageViewModels
{
    class PasswordManagerViewModel : ReactiveObject
    {
        public PasswordManagerViewModel()
        {
            this.WhenAnyValue(x => x.SearchQuery)
                 .Throttle(TimeSpan.FromMilliseconds(100))
                 .Where(term => null != term)
                 .DistinctUntilChanged()
                 .InvokeCommand(FilterAccountCommand);

            this.WhenAnyValue(x => x.Device)
                .Where(x => null != x)
                .InvokeCommand(FilterAccountCommand);

            this.WhenAnyValue(x => x.SelectedAccount)
                .InvokeCommand(CancelCommand);

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Accounts, nameof(ObservableCollection<string>.CollectionChanged))
                      .Subscribe(change => SelectedAccount = Accounts.FirstOrDefault());
        }

        [Reactive] public DeviceViewModel Device { get; set; }
        [Reactive] public AccountInfoViewModel SelectedAccount { get; set; }
        [Reactive] public EditAccountViewModel EditAccount { get; set; }
        [Reactive] public string SearchQuery { get; set; }
        public ObservableCollection<AccountInfoViewModel> Accounts { get; } = new ObservableCollection<AccountInfoViewModel>();

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
        public ICommand DeleteAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnDeleteAccount();
                    },
                };
            }
        }

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

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnCancel();
                    },
                };
            }
        }

        public ICommand SaveAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnSaveAccount((x as PasswordBox)?.SecurePassword);
                    },
                };
            }
        }

        public ICommand FilterAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Task.Run(OnFilterAccount);
                    },
                };
            }
        }

        #endregion

        private void OnSaveAccount(SecureString password)
        {
            string message ="";

            message += $"Account Name: {EditAccount.Name}\n";
            message += $"Login: {EditAccount.Login}\n";
            message += $"Password: {password}\n";
            message += $"AppsAndUrls: {string.Join(";", EditAccount.AppsAndUrls)}\n";
            message += $"OtpSecret: {EditAccount.OtpSecret}\n";


            MessageBox.Show(message);
        }

        private void OnAddAccount()
        {
            EditAccount = new EditAccountViewModel(Device);
        }

        private void OnDeleteAccount()
        {
            Accounts.Remove(SelectedAccount);
            EditAccount = null;
        }

        private void OnEditAccount()
        {
            if (Device.AccountsRecords.TryGetValue(SelectedAccount.Key, out AccountRecord record))
            {
                EditAccount = new EditAccountViewModel(Device, record);
            }
        }

        private void OnCancel()
        {
            EditAccount = null;
        }

        private void OnFilterAccount()
        {
            var filteredAccounts = Device.Accounts.Where(a => Contains(a, SearchQuery));
            Application.Current.Dispatcher.Invoke(() =>
            {
                Accounts.RemoveMany(Accounts.Except(filteredAccounts));
                Accounts.AddRange(filteredAccounts.Except(Accounts));
            });
        }
        private bool Contains(AccountInfoViewModel account, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            if (null != account)
            {
                var filter = value.Trim();
                return Contains(account.Name, filter) || Contains(account.Login, filter) || account.AppsUrls.Any(a => Contains(a, filter));
            }

            return false;
        }
        private bool Contains(string source, string toCheck)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
