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
