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

        public DeviceViewModel(IDevice device)
        {
            Device = device;

            Device.ConnectionStateChanged += (object sender, EventArgs e) 
                => NotifyPropertyChanged(nameof(IsConnected));
        }

    }
}
