using HideezClient.Models;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class ActiveDeviceChangedMessage: PubSubMessageBase
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
