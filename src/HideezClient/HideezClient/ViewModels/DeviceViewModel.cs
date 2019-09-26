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
            this.device = device;
           // device.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);

            StorageLoadedStateChanged();
            //bindingRaiseeventIsStorageLoaded = new BindingRaiseevent(device, nameof(device.IsStorageLoaded));
            //bindingRaiseeventIsStorageLoaded.ValueChanged += obj => StorageLoadedStateChanged();
        }
        private void StorageLoadedStateChanged()
        {
           // if (device.IsStorageLoaded && device.PasswordManager != null)
            {
                Accounts.Clear();

                // TODO: Del
                var Testlist = new[]
                {
                    new AccountRecord {  Name = "Pizza Hut", Login = "john.gardner@example.com", Urls="google.com;google.com.ua" },
                    new AccountRecord {  Name = "The Walt Disney Company", Login = "seth.olson@example.com", },
                    new AccountRecord {  Name = "Bank of America", Login = "penny.nichols@example.com", },
                    new AccountRecord {  Name = "eBay", Login = "alice.bryant@example.com", Apps = "Skype;WinRar", Urls="google.com;google.com.ua" },
                    new AccountRecord { Name = "MasterCard", Login = "tamara.kuhn@example.com", Flags = (ushort)StorageTableFlags.HAS_OTP, },
                    new AccountRecord {  Name = "Johnson & Johnson", Login = "keith.richards@example.com" },
                    new AccountRecord {  Name = "Starbucks", Login = "logan.hopkins@example.com", },
                    new AccountRecord {  Name = "Facebook", Login = "kelly.howard@example.com", },
                    new AccountRecord { Name = "Mitsubishi", Login = "dan.romero@example.com", Flags = (ushort)StorageTableFlags.HAS_OTP, },
                    new AccountRecord {  Name = "Apple", Login = "gary.herrera@example.com", },
                    new AccountRecord {  Name = "Louis Vuitton", Login = "jessica.hanson@example.com", },
                };

                foreach (var acc in Testlist)
                // foreach (var acc in device.PasswordManager.Accounts.Values)
                {
                    Accounts.Add(new CredentialsInfoViewModel(acc));
                }
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
        public bool IsFaulted => device.IsFaulted;
        public string FaultMessage => device.FaultMessage;
        public ObservableCollection<CredentialsInfoViewModel> Accounts { get; } = new ObservableCollection<CredentialsInfoViewModel>();
    }
}
