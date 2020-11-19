using HideezClient.Models;

namespace HideezClient.Modules
{
    class ActiveDeviceChangedEventArgs
    {
        public DeviceModel PreviousDevice { get; }

        public DeviceModel NewDevice { get; }

        public ActiveDeviceChangedEventArgs(DeviceModel previousDevice, DeviceModel newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }

    delegate void ActiveDeviceChangedEventHandler(object sender, ActiveDeviceChangedEventArgs args);

    interface IActiveDevice
    {
        event ActiveDeviceChangedEventHandler ActiveDeviceChanged;

        DeviceModel Device { get; set; }
    }
}
