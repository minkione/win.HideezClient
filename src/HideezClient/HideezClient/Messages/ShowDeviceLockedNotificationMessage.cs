using HideezClient.Models;
using HideezClient.Modules;

namespace HideezClient.Messages
{
    class ShowDeviceLockedNotificationMessage
    {
        public HardwareVaultModel Device{ get; set; }

        public ShowDeviceLockedNotificationMessage(HardwareVaultModel device)
        {
            Device = device;
        }
    }
}
