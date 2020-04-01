using HideezClient.Models;

namespace HideezClient.Modules
{
    class ActiveDeviceChangedEventArgs
    {
        public HardwareVaultModel PreviousDevice { get; }

        public HardwareVaultModel NewDevice { get; }

        public ActiveDeviceChangedEventArgs(HardwareVaultModel previousDevice, HardwareVaultModel newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }

    delegate void ActiveDeviceChangedEventHandler(object sender, ActiveDeviceChangedEventArgs args);

    interface IActiveDevice
    {
        event ActiveDeviceChangedEventHandler ActiveDeviceChanged;

        HardwareVaultModel Device { get; set; }
    }
}
