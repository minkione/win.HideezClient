using HideezClient.Models;
using HideezClient.Modules;

namespace HideezClient.Messages
{
    class ShowDeviceLockedNotificationMessage
    {
        public Device Device{ get; set; }

        public ShowDeviceLockedNotificationMessage(Device device)
        {
            Device = device;
        }
    }
}
