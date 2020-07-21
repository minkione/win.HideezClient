using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class RemoveDeviceMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public RemoveDeviceMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
