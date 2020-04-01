using HideezClient.Models;

namespace HideezClient.Messages
{
    class ActiveDeviceChangedMessage
    {
        public IVaultModel PreviousDevice { get; }
        
        public IVaultModel NewDevice { get; }

        public ActiveDeviceChangedMessage(IVaultModel previousDevice, IVaultModel newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }
}
