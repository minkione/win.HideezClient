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

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Accounts, nameof(ObservableCollection<string>.CollectionChanged))
                      .Subscribe(change => SelectedCredentials = Accounts.FirstOrDefault());
        }

        [Reactive]
        public bool IsInfoMode { get; set; }
        [Reactive]
        public DeviceViewModel Device { get; set; }
        [Reactive]
        public CredentialsInfoViewModel SelectedCredentials { get; set; }
        [Reactive]
        public string SearchQuery { get; set; }
        public ObservableCollection<CredentialsInfoViewModel> Accounts { get; } = new ObservableCollection<CredentialsInfoViewModel>();

        private bool Contains(CredentialsInfoViewModel account, string value)
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
                        OnAddAccount();
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
                        OnAddAccount();
                    },
                };
            }
        }
        public ICommand CancelAccountCommand
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
        public ICommand FilterAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnFilterAccount();
                    },
                };
            }
        }

        #endregion

        private void OnFilterAccount()
        {
            var filteredAccounts = Device.Accounts.Where(a => Contains(a, SearchQuery));
            Application.Current.Dispatcher.Invoke(() =>
            {
                Accounts.RemoveMany(Accounts.Except(filteredAccounts));
                Accounts.AddRange(filteredAccounts.Except(Accounts));
            });
        }

        private void OnAddAccount()
        {
            IsInfoMode = false;
            var editViewModwl = new EditCredentialsViewModel();
        }
    }
}
