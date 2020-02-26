﻿using HideezClient.Models;
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
using HideezClient.Utilities.QrCode;
using HideezClient.Modules;
using Hideez.SDK.Communication;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Modules.Log;
using Hideez.SDK.Communication.Log;

namespace HideezClient.PageViewModels
{
    class PasswordManagerViewModel : ReactiveObject
    {
        // TODO: Optimize AccountRecord reading

        readonly Logger log = LogManager.GetCurrentClassLogger(nameof(PasswordManagerViewModel));
        readonly IQrScannerHelper qrScannerHelper;
        readonly IWindowsManager windowsManager;
        readonly IMessenger _messenger;

        public PasswordManagerViewModel(IWindowsManager windowsManager, IQrScannerHelper qrScannerHelper, IMessenger messenger, IActiveDevice activeDevice)
        {
            this.windowsManager = windowsManager;
            this.qrScannerHelper = qrScannerHelper;
            _messenger = messenger;

            _messenger.Register<ActiveDeviceChangedMessage>(this, OnActiveDeviceChanged);
            _messenger.Register<AddAccountForAppMessage>(this, OnAddAccountForApp);

            this.WhenAnyValue(x => x.SearchQuery)
                 .Throttle(TimeSpan.FromMilliseconds(100))
                 .Where(term => null != term)
                 .DistinctUntilChanged()
                 .InvokeCommand(FilterAccountCommand);

            this.WhenAnyValue(x => x.Device)
                .Where(x => null != x)
                .Subscribe(d => OnDeviceChanged());

            this.WhenAnyValue(x => x.SelectedAccount)
                .InvokeCommand(CancelCommand);

            // Todo: On update, preserve selection or if unable, clear it
            //Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Accounts, nameof(ObservableCollection<string>.CollectionChanged))
            //          .Subscribe(change => SelectedAccount = Accounts.FirstOrDefault());

            Device = activeDevice.Device != null ? new DeviceViewModel(activeDevice.Device) : null;
        }

        [Reactive] public bool IsAvailable { get; set; }
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
                        OnDeleteAccountAsync();
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
                        if (EditAccount.CanSave())
                        {
                            OnSaveAccountAsync((x as PasswordBox)?.SecurePassword);
                        }
                    },
                    CanExecuteFunc = () => EditAccount != null && EditAccount.HasChanges && !EditAccount.HasError,
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

        private void OnActiveDeviceChanged(ActiveDeviceChangedMessage obj)
        {
            // Todo: ViewModel should be reused instead of being recreated each time active device is changed
            Device = obj.NewDevice != null ? new DeviceViewModel(obj.NewDevice) : null;
        }

        private void OnAddAccountForApp(AddAccountForAppMessage obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var vm = new EditAccountViewModel(Device, windowsManager, qrScannerHelper)
                {
                    DeleteAccountCommand = this.DeleteAccountCommand,
                    CancelCommand = this.CancelCommand,
                    SaveAccountCommand = this.SaveAccountCommand,
                };
                vm.Name = obj.AppInfo.Title;
                vm.AppsAndUrls.Add(new AppViewModel(obj.AppInfo.Title, !string.IsNullOrWhiteSpace(obj.AppInfo.Domain)));
                EditAccount = vm;
            });
        }

        private async Task OnSaveAccountAsync(SecureString password)
        {
            try
            {
                var account = EditAccount;
                EditAccount = null;
                IsAvailable = false;
                account.AccountRecord.Password = password.GetAsString();
                await Device.SaveOrUpdateAccountAsync(account.AccountRecord);
                OnFilterAccount();
            }
            catch (Exception ex)
            {
                IsAvailable = true;
                HandleError(ex, "Error save account.");
            }
        }

        private void OnDeviceChanged()
        {
            OnFilterAccount();
            PropertyChangedEventManager.AddHandler(Device, (s, e) => OnFilterAccount(), nameof(DeviceViewModel.AccountsRecords));
        }

        private void OnAddAccount()
        {
            EditAccount = new EditAccountViewModel(Device, windowsManager, qrScannerHelper)
            {
                DeleteAccountCommand = this.DeleteAccountCommand,
                CancelCommand = this.CancelCommand,
                SaveAccountCommand = this.SaveAccountCommand,
            };
        }

        private async Task OnDeleteAccountAsync()
        {
            if (await windowsManager.ShowDeleteCredentialsPromptAsync())
            {
                IsAvailable = false;
                try
                {
                    EditAccount = null;
                    await Device.DeleteAccountAsync(SelectedAccount.AccountRecord);
                    OnFilterAccount();
                }
                catch (Exception ex)
                {
                    IsAvailable = true;
                    HandleError(ex, "Error delete account.");
                }
            }
        }

        private void OnEditAccount()
        {
            if (Device.AccountsRecords.TryGetValue(SelectedAccount.Key, out AccountRecord record))
            {
                EditAccount = new EditAccountViewModel(Device, record, windowsManager, qrScannerHelper)
                {
                    DeleteAccountCommand = this.DeleteAccountCommand,
                    CancelCommand = this.CancelCommand,
                    SaveAccountCommand = this.SaveAccountCommand,
                };
            }
        }

        private void OnCancel()
        {
            EditAccount = null;
        }

        private void OnFilterAccount()
        {
            var filteredAccounts = Device.AccountsRecords.Select(r => new AccountInfoViewModel(r.Value)).Where(a => a.IsVisible).Where(a => Contains(a, SearchQuery));
            filteredAccounts = filteredAccounts.OrderBy(a => a.Name).OrderByDescending(a => a.IsEditable); // Editable accounts will be shown first in the list
            Application.Current.Dispatcher.Invoke(() =>
            {
                Accounts.RemoveMany(Accounts.Except(filteredAccounts));
                Accounts.AddRange(filteredAccounts.Except(Accounts));
            });

            IsAvailable = true;
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

        private void HandleError(Exception ex, string message)
        {
            log.WriteLine(ex);
            try
            {
                if (ex is HideezException hex)
                {
                    if (hex.ErrorCode == HideezErrorCode.ERR_UNAUTHORIZED)
                    {
                        windowsManager.ShowError("Authorization error.");
                    }
                    else if (hex.ErrorCode == HideezErrorCode.PmPasswordNameCannotBeEmpty)
                    {
                        windowsManager.ShowError("Account name cannot be empty.");
                    }
                }
                else
                {
                    windowsManager.ShowError(message);
                }
            }
            catch (Exception ManagerEx)
            {
                log.WriteLine(ManagerEx);
            }
        }
    }
}