using HideezClient.Models;

namespace HideezClient.Messages
{
    class ShowDeviceLockedByPinNotificationMessage
    {
        public DeviceModel Device { get; set; }

        public ShowDeviceLockedByPinNotificationMessage(DeviceModel device)
        {
            Device = device;
        }
    }
}
