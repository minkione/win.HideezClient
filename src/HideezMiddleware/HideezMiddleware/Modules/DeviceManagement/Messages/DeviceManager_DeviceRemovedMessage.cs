using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.DeviceManagement.Messages
{
    public sealed class DeviceManager_DeviceRemovedMessage : PubSubMessageBase
    {
        public DeviceManager Sender { get; }

        public IDevice Device { get; }

        public DeviceManager_DeviceRemovedMessage(DeviceManager sender, IDevice device)
        {
            Sender = sender;
            Device = device;
        }
    }
}
