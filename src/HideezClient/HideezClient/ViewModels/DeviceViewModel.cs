using Hideez.SDK.Communication.BLE;
using HideezClient.Models;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using NLog;
using System;
using System.Collections.Generic;
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

        public DeviceViewModel(Device device)
        {
            this.device = device;
            device.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);
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
                            await device.AuthorizeAndLoadStorage();
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
        public Version FirmwareVersion => device.FirmwareVersion;
        public Version BootloaderVersion => device.BootloaderVersion;
        public uint StorageTotalSize => device.StorageTotalSize;
        public uint StorageFreeSize => device.StorageFreeSize;
        public bool FinishedMainFlow => device.FinishedMainFlow;
        public bool IsCreatingRemoteDevice => device.IsCreatingRemoteDevice;
        public bool IsAuthorizingRemoteDevice => device.IsAuthorizingRemoteDevice;
    }
}
