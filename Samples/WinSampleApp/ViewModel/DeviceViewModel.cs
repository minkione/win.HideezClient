using System;
using Hideez.SDK.Communication.Interfaces;

namespace WinSampleApp.ViewModel
{
    public class DeviceViewModel : ViewModelBase
    {
        public IDevice Device { get; }

        public string Id => Device.Id;
        public string Name => Device.Name;
        public string Mac => Device.Mac;
        public bool IsConnected => Device.IsConnected;
        public int ChannelNo => Device.ChannelNo;

        public string SerialNo => Device.SerialNo;
        public Version FirmwareVersion => Device.FirmwareVersion;
        public Version BootloaderVersion => Device.BootloaderVersion;
        public uint StorageTotalSize => Device.StorageTotalSize;
        public uint StorageFreeSize => Device.StorageFreeSize;
        public bool IsInitialized => Device.IsInitialized;
        public int Battery => Device.Battery;
        public bool IsLinkRequired => Device.IsLinkRequired;
        public bool IsNewPinRequired => Device.IsNewPinRequired;
        public bool IsMasterKeyRequired => Device.IsMasterKeyRequired;
        public bool IsPinRequired => Device.IsPinRequired;
        public bool IsButtonRequired => Device.IsButtonRequired;
        public bool IsLocked => Device.IsLocked;
        public byte WrongPinAttempts => Device.WrongPinAttempts;
        public DateTime DeviceTime => Device.DeviceTime;
        public ushort MaxMessageSize => Device.MaxMessageSize;

        private double updateFwProgress;
        public double UpdateFwProgress
        {
            get { return updateFwProgress; }
            set
            {
                if (updateFwProgress != value)
                {
                    updateFwProgress = value;
                    NotifyPropertyChanged(nameof(UpdateFwProgress));
                }
            }
        }

        public DeviceViewModel(IDevice device)
        {
            Device = device;

            Device.ConnectionStateChanged += (object sender, EventArgs e) 
                => NotifyPropertyChanged(nameof(IsConnected));

            Device.PropertyChanged += Device_PropertyChanged;
        }

        private void Device_PropertyChanged(object sender, string e)
        {
            NotifyPropertyChanged(e);
        }
    }
}
