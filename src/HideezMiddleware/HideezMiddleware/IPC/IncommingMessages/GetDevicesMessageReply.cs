using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class GetDevicesMessageReply : PubSubMessageBase
    {
        public DeviceDTO[] Devices { get; set; }

        public GetDevicesMessageReply(DeviceDTO[] devices)
        {
            Devices = devices;
        }
    }
}
