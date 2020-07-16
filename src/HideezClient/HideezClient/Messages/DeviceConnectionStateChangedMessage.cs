using HideezMiddleware.IPC.DTO;

namespace HideezClient.Messages
{
    class DeviceConnectionStateChangedMessage
    {
        public DeviceConnectionStateChangedMessage(DeviceDTO device)
        {
            Device = device;
        }

        public DeviceDTO Device { get; }
    }
}
