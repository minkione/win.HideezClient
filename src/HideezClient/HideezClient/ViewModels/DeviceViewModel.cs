using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Models;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Device;

namespace HideezClient.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        protected DeviceModel device;

        public DeviceViewModel(DeviceModel device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            
            this.device = device;
            device.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);
        }
        public string Name => device.Name;
        public string SerialNo => device.SerialNo;
        public string OwnerName => device.OwnerName;
        public string OwnerEmail => device.OwnerEmail;
        public string Id => device.Id;
        public string Mac => device.Mac;
        public bool IsConnected => device.IsConnected;
        public bool CanRemoveConnection => device.CanRemoveConnection;
        public bool CanDisconnect => device.CanDisconnect && device.IsConnected;
        public double Proximity => device.Proximity;
        public int Battery => device.Battery;
        public bool IsInitializing => device.IsInitializing;
        public bool IsInitialized => device.IsInitialized;
        public bool IsAuthorizing => device.IsAuthorizingRemoteDevice;
        public bool IsAuthorized => device.IsAuthorized;
        public bool IsLoadingStorage => device.IsLoadingStorage;
        public bool IsStorageLoaded => device.IsStorageLoaded;
        [DependsOn(nameof(IsConnected), nameof(IsInitialized), nameof(IsAuthorized), nameof(IsStorageLoaded), nameof(IsStorageLocked))]
        public bool CanAccessStorage { get { return IsConnected && IsInitialized && IsAuthorized && IsStorageLoaded && !IsStorageLocked; } }
        public Version FirmwareVersion => device.FirmwareVersion;
        public Version BootloaderVersion => device.BootloaderVersion;
        public uint StorageTotalSize => device.StorageTotalSize;
        public uint StorageFreeSize => device.StorageFreeSize;
        [DependsOn(nameof(StorageTotalSize))]
        public uint StorageTotalSizeKb => StorageTotalSize / 1024;
        [DependsOn(nameof(StorageFreeSize), nameof(StorageTotalSize))]
        public byte StorageFreePercent => (byte)(((double)StorageFreeSize / StorageTotalSize) * 100);
        [DependsOn(nameof(IsStorageLoaded))]
        public IEnumerable<AccountRecord> AccountsRecords 
        { 
            get 
            {
                if (device?.PasswordManager != null)
                    return device.PasswordManager.Accounts;
                else
                    return new List<AccountRecord>();
            } 
        }
        public bool CanLockByProximity => device.CanLockByProximity;
        public bool IsStorageLocked => device.IsStorageLocked;
        public bool FinishedMainFlow => device.FinishedMainFlow;
        public bool IsCreatingRemoteDevice => device.IsCreatingRemoteDevice;
        public bool IsAuthorizingRemoteDevice => device.IsAuthorizingRemoteDevice;

        public async Task SaveOrUpdateAccountAsync(AccountRecord account, bool isReadonly = false)
        {
            var flags = new AccountFlagsOptions();
            if (isReadonly)
                flags.IsUnlockAccount = true;
            else flags.IsUserAccount = true;

            if (account.StorageId == null)
                account.StorageId = new StorageId();
            
            account.Timestamp = ConvertUtils.ConvertToUnixTime(DateTime.UtcNow);

           await device.PasswordManager.SaveOrUpdateAccount(
               account.StorageId, 
               account.Timestamp,
               account.Name, 
               account.Password, 
               account.Login, 
               account.OtpSecret, 
               account.Apps, 
               account.Urls, 
               account.IsPrimary, 
               flags);
        }

        public Task DeleteAccountAsync(AccountRecord account)
        {
            return device.PasswordManager.DeleteAccount(account.StorageId, account.IsPrimary);
        }

        public Task<AccessParams> GetAccessProfile()
        {
            return device.GetAccessProfile();
        }

        public Task<bool> ChangeMasterPassword()
        {
            return device.ChangeMasterkeyWorkflow();
        }

        public Task<bool> ChangePinCode()
        {
            return device.ChangePinWorkflow();
        }

        public Task<bool> ChangeAccessProfile(bool requirePin, bool requireButton, int expirationSeconds)
        {
            return device.ChangeAccessProfileWorkflow(requirePin, requireButton, expirationSeconds);
        }

        public Task<bool> Wipe()
        {
            return device.WipeWorkflow();
        }
    }
}
