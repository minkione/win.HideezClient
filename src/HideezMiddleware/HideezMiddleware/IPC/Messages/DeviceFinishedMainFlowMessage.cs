using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceFinishedMainFlowMessage : PubSubMessageBase
    {
        public DeviceDTO Device { get; }

        public DeviceFinishedMainFlowMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
