using System;
using Hideez.SDK.Communication.BLE;

namespace WinSampleApp.ViewModel
{
    public class DeviceViewModel : ViewModelBase
    {
        public BleDevice Device { get; }

        public string Id => Device.Id;
        public string Name => Device.Name;
        public string Mac => Device.Mac;
        public bool IsConnected => Device.IsConnected;
        public int ChannelNo => Device.ChannelNo;

        public DeviceViewModel(BleDevice device)
        {
            Device = device;

            Device.ConnectionStateChanged += (object sender, EventArgs e) 
                => NotifyPropertyChanged(nameof(IsConnected));
        }

    }
}
