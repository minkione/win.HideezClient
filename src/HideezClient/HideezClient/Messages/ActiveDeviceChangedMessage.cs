using HideezClient.Models;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class ActiveDeviceChangedMessage: PubSubMessageBase
    {
        public DeviceModel PreviousDevice { get; }
        
        public DeviceModel NewDevice { get; }

        public ActiveDeviceChangedMessage(DeviceModel previousDevice, DeviceModel newDevice)
        {
            PreviousDevice = previousDevice;
            NewDevice = newDevice;
        }
    }
}
