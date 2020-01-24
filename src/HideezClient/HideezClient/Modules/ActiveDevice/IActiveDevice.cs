using HideezClient.Models;

namespace HideezClient.Modules
{
    class ActiveDeviceChangedEventArgs
    {
        public Device PreviousDevice { get; }

        public Device NewDevice { get; }

        public ActiveDeviceChangedEventArgs(Device previousDevice, Device newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }

    delegate void ActiveDeviceChangedEventHandler(object sender, ActiveDeviceChangedEventArgs args);

    interface IActiveDevice
    {
        event ActiveDeviceChangedEventHandler ActiveDeviceChanged;

        Device Device { get; set; }
    }
}
