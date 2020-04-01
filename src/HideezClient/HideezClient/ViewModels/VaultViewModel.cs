using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.BLE;
using HideezClient.Models;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HideezClient.ViewModels
{
    public class VaultViewModel : LocalizedObject
    {
        protected HardwareVaultModel _vault;

        public VaultViewModel(HardwareVaultModel vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            _vault = vault;
            vault.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);
        }
        public string Name => _vault.Name;
        public string SerialNo => _vault.SerialNo;
        public string OwnerName => _vault.OwnerName;
        public string OwnerEmail => _vault.OwnerEmail;
        public string Id => _vault.Id;
        public string Mac => BleUtils.DeviceIdToMac(Id);
        public bool IsConnected => _vault.IsConnected;
        public double Proximity => _vault.Proximity;
        public int Battery => _vault.Battery;
        public bool IsInitializing => _vault.IsInitializing;
        public bool IsInitialized => _vault.IsInitialized;
        public bool IsAuthorizing => _vault.IsAuthorizingRemoteDevice;
        public bool IsAuthorized => _vault.IsAuthorized;
        public bool IsLoadingStorage => _vault.IsLoadingStorage;
        public bool IsStorageLoaded => _vault.IsStorageLoaded;
        [DependsOn(nameof(IsConnected), nameof(IsInitialized), nameof(IsAuthorized), nameof(IsStorageLoaded))]
        public bool CanShowPasswordManager { get { return IsConnected && IsInitialized && IsAuthorized && IsStorageLoaded; } }
        public Version FirmwareVersion => _vault.FirmwareVersion;
        public Version BootloaderVersion => _vault.BootloaderVersion;
        public uint StorageTotalSize => _vault.StorageTotalSize;
        public uint StorageFreeSize => _vault.StorageFreeSize;
        [DependsOn(nameof(StorageTotalSize))]
        public uint StorageTotalSizeKb => StorageTotalSize / 1024;

        [DependsOn(nameof(StorageFreeSize), nameof(StorageTotalSize))]
        public byte StorageFreePercent => (byte)(((double)StorageFreeSize / StorageTotalSize) * 100);

        [DependsOn(nameof(IsStorageLoaded))]
        public IDictionary<ushort, AccountRecord> AccountsRecords 
        { 
            get 
            {
                if (_vault?.PasswordManager != null)
                    return _vault.PasswordManager.Accounts;
                else
                    return new Dictionary<ushort, AccountRecord>();
            } 
        }

        public bool CanLockByProximity => _vault.CanLockByProximity;

        public Task<ushort> SaveOrUpdateAccountAsync(AccountRecord account)
        {
            var flags = new AccountFlagsOptions
            {
                IsUserAccount = true,
            };
            return _vault.PasswordManager.SaveOrUpdateAccount(
                account.Key, 
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
            return _vault.PasswordManager.DeleteAccount(account.Key, account.IsPrimary);
        }
        public bool FinishedMainFlow => _vault.FinishedMainFlow;
        public bool IsCreatingRemoteDevice => _vault.IsCreatingRemoteDevice;
        public bool IsAuthorizingRemoteDevice => _vault.IsAuthorizingRemoteDevice;
    }
}
