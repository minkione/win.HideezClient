using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DevicesCollectionChangedMessage : PubSubMessageBase
    {
        public DeviceDTO[] Devices { get; }

        public DevicesCollectionChangedMessage(DeviceDTO[] devices)
        {
            Devices = devices;
        }
    }
}
