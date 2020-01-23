using DynamicData;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Controls;
using HideezClient.Extension;
using Hideez.SDK.Communication.BLE;

using HideezClient.Models;
using HideezClient.Modules.Localize;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        protected Device device;
        protected readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private WeakPropertyObserver bindingRaiseeventIsStorageLoaded;

        public DeviceViewModel(Device device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));



            this.device = device;
            device.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);

            StorageLoadedStateChanged();
            bindingRaiseeventIsStorageLoaded = new WeakPropertyObserver(device, nameof(device.IsStorageLoaded));
            bindingRaiseeventIsStorageLoaded.ValueChanged += (propName, obj) => StorageLoadedStateChanged();
        }
        private void StorageLoadedStateChanged()
        {
            if (device.IsStorageLoaded && device.PasswordManager != null)
            {
                Accounts.Clear();
                Accounts.AddRange(device.PasswordManager.Accounts.Values.Select(acc => new AccountInfoViewModel(acc)));
            }
        }

        public ICommand AuthorizeDeviceAndLoadStorage
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async (x) =>
                    {
                        try
                        {
                            await device.InitRemoteAndLoadStorageAsync();
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex);
                        }
                    }
                };
            }
        }

        public string Name => device.Name;
        public string SerialNo => device.SerialNo;
        public string OwnerName => device.OwnerName;
        public string Id => device.Id;
        public string Mac => BleUtils.DeviceIdToMac(Id);
        public bool IsConnected => device.IsConnected;
        public double Proximity => device.Proximity;
        public int Battery => device.Battery;
        public bool IsInitializing => device.IsInitializing;
        public bool IsInitialized => device.IsInitialized;
        public bool IsAuthorizing => device.IsAuthorizingRemoteDevice;
        public bool IsAuthorized => device.IsAuthorized;
        public bool IsLoadingStorage => device.IsLoadingStorage;
        public bool IsStorageLoaded => device.IsStorageLoaded;
        [DependsOn(nameof(IsConnected), nameof(IsInitialized), nameof(IsAuthorized))]
        public bool CanShowPasswordManager { get { return IsConnected && IsInitialized && IsAuthorized && IsAuthorized; } }
        public Version FirmwareVersion => device.FirmwareVersion;
        public Version BootloaderVersion => device.BootloaderVersion;
        public uint StorageTotalSize => device.StorageTotalSize;
        public uint StorageFreeSize => device.StorageFreeSize;
        [DependsOn(nameof(StorageTotalSize))]
        public uint StorageTotalSizeKb => StorageTotalSize / 1024;

        [DependsOn(nameof(StorageFreeSize), nameof(StorageTotalSize))]
        public byte StorageFreePercent => (byte)(((double)StorageFreeSize / StorageTotalSize) * 100);

        public IDictionary<ushort, AccountRecord> AccountsRecords 
        { 
            get 
            {
                if (device?.PasswordManager != null)
                    return device.PasswordManager.Accounts;
                else
                    return new Dictionary<ushort, AccountRecord>();
            } 
        }
        public ObservableCollection<AccountInfoViewModel> Accounts { get; } = new ObservableCollection<AccountInfoViewModel>();

        public bool CanLockByProximity => device.CanLockByProximity;

        public Task<ushort> SaveOrUpdateAccountAsync(AccountRecord account)
        {
            return device.PasswordManager.SaveOrUpdateAccount(account.Key, account.Name, account.Password, account.Login, account.OtpSecret, account.Apps, account.Urls, account.IsPrimary);
        }

        public Task DeleteAccountAsync(AccountRecord account)
        {
            return device.PasswordManager.DeleteAccount(account.Key, account.IsPrimary);
        }
        public bool FinishedMainFlow => device.FinishedMainFlow;
        public bool IsCreatingRemoteDevice => device.IsCreatingRemoteDevice;
        public bool IsAuthorizingRemoteDevice => device.IsAuthorizingRemoteDevice;
    }
}
