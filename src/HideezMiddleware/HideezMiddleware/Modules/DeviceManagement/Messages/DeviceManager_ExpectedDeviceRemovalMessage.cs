using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.DeviceManagement.Messages
{
    class DeviceManager_ExpectedDeviceRemovalMessage : PubSubMessageBase
    {
        public IDevice Device { get; }

        public DeviceManager_ExpectedDeviceRemovalMessage(IDevice device)
        {
            Device = device;
        }
    }
}
