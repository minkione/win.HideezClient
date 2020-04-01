using HideezClient.Models;

namespace HideezClient.Messages
{
    class ActiveDeviceChangedMessage
    {
        public HardwareVaultModel PreviousDevice { get; }
        
        public HardwareVaultModel NewDevice { get; }

        public ActiveDeviceChangedMessage(HardwareVaultModel previousDevice, HardwareVaultModel newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }
}
