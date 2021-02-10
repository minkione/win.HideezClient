using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public class DeviceDisconnectedMessage : PubSubMessageBase
    {
        public DeviceDTO Device { get; }

        public DeviceDisconnectedMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
