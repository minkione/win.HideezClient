using HideezMiddleware.IPC.DTO;

namespace HideezClient.Messages
{
    class DeviceFinishedMainFlowMessage
    {
        public DeviceDTO Device { get; }

        public DeviceFinishedMainFlowMessage(DeviceDTO device)
        {
            Device = device;
        }
    }
}
