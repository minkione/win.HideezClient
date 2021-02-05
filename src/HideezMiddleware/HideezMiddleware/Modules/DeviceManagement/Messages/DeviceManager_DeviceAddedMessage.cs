using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.DeviceManagement.Messages
{
    public sealed class DeviceManager_DeviceAddedMessage : PubSubMessageBase
    {
        public DeviceManager Sender { get; }

        public IDevice Device { get; }

        public DeviceManager_DeviceAddedMessage(DeviceManager sender, IDevice device)
        {
            Sender = sender;
            Device = device;
        }
    }
}
