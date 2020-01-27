using HideezClient.Models;

namespace HideezClient.Messages
{
    class ActiveDeviceChangedMessage
    {
        public Device PreviousDevice { get; }
        
        public Device NewDevice { get; }

        public ActiveDeviceChangedMessage(Device previousDevice, Device newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }
}
