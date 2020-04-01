using HideezClient.Models;

namespace HideezClient.Modules
{
    class ActiveDeviceChangedEventArgs
    {
        public IVaultModel PreviousDevice { get; }

        public IVaultModel NewDevice { get; }

        public ActiveDeviceChangedEventArgs(IVaultModel previousDevice, IVaultModel newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }

    delegate void ActiveDeviceChangedEventHandler(object sender, ActiveDeviceChangedEventArgs args);

    interface IActiveDevice
    {
        event ActiveDeviceChangedEventHandler ActiveDeviceChanged;

        IVaultModel Vault { get; set; }
    }
}
