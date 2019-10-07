using DynamicData;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Controls;
using HideezClient.Models;
using HideezClient.Mvvm;
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
    class DeviceViewModel : LocalizedObject
    {
        protected Device device;
        protected readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private BindingRaiseevent bindingRaiseeventIsStorageLoaded;

        public DeviceViewModel(Device device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            this.device = device;
            device.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);

            StorageLoadedStateChanged();
            bindingRaiseeventIsStorageLoaded = new BindingRaiseevent(device, nameof(device.IsStorageLoaded));
            bindingRaiseeventIsStorageLoaded.ValueChanged += obj => StorageLoadedStateChanged();
        }
        private void StorageLoadedStateChanged()
        {
            if (device.IsStorageLoaded && device.PasswordManager != null)
            {
                Accounts.Clear();

                // TODO: Del
                //AccountsRecords = new Dictionary<ushort, AccountRecord>()
                //{
                //    { 1, new AccountRecord {  Name = "Pizza Hut", Login = "john.gardner@example.com", Urls="google.com;google.com.ua" } },
                //    { 2, new AccountRecord {  Name = "The Walt Disney Company", Login = "seth.olson@example.com", } },
                //    { 3, new AccountRecord {  Name = "Bank of America", Login = "penny.nichols@example.com", } },
                //    { 4, new AccountRecord {  Name = "eBay", Login = "alice.bryant@example.com", Apps = "Skype;WinRar", Urls="google.com;google.com.ua" } },
                //    { 5, new AccountRecord { Name = "MasterCard", Login = "tamara.kuhn@example.com", Flags = (ushort)StorageTableFlags.HAS_OTP, } },
                //    { 6, new AccountRecord {  Name = "Johnson & Johnson", Login = "keith.richards@example.com" } },
                //    { 7, new AccountRecord {  Name = "Starbucks", Login = "logan.hopkins@example.com", } },
                //    { 8, new AccountRecord {  Name = "Facebook", Login = "kelly.howard@example.com", } },
                //    { 9, new AccountRecord { Name = "Mitsubishi", Login = "dan.romero@example.com", Flags = (ushort)StorageTableFlags.HAS_OTP, } },
                //    { 10, new AccountRecord {  Name = "Apple", Login = "gary.herrera@example.com", } },
                //    { 11, new AccountRecord {  Name = "Louis Vuitton", Login = "jessica.hanson@example.com", } },
                //};

                //foreach (var acc in AccountsRecords)
                //    acc.Value.Key = acc.Key;
                Accounts.AddRange(device.PasswordManager.Accounts.Values.Select(acc => new AccountInfoViewModel(acc)));
            }
        }

        public string Name => device.Name;
        public string SerialNo => device.SerialNo;
        public string OwnerName => device.OwnerName;
        public string Id => device.Id;
        public string Mac => Id.Split(':').FirstOrDefault();
        public bool IsConnected => device.IsConnected;
        public double Proximity => device.Proximity;
        public int Battery => device.Battery;
        public bool IsInitializing => device.IsInitializing;
        public bool IsInitialized => device.IsInitialized;
        public bool IsAuthorizing => device.IsAuthorizing;
        public bool IsAuthorized => device.IsAuthorized;
        public bool IsLoadingStorage => device.IsLoadingStorage;
        public bool IsStorageLoaded => device.IsStorageLoaded;
        public Version FirmwareVersion => device.FirmwareVersion;
        public Version BootloaderVersion => device.BootloaderVersion;
        public uint StorageTotalSize => device.StorageTotalSize;
        public uint StorageFreeSize => device.StorageFreeSize;
        public IDictionary<ushort, AccountRecord> AccountsRecords { get { return device.PasswordManager.Accounts; } }
        public ObservableCollection<AccountInfoViewModel> Accounts { get; } = new ObservableCollection<AccountInfoViewModel>();

        public Task<ushort> SaveOrUpdateAccount(ushort key, string name, string password,
           string login, string otpSecret, string apps, string urls, bool isPrimary = false,
           ushort flags = 0, ushort flagsMask = 0)
        {
            return device.PasswordManager.SaveOrUpdateAccount(key, name, password, login, otpSecret, apps, urls, isPrimary, flags, flagsMask);
        }
    }
}
