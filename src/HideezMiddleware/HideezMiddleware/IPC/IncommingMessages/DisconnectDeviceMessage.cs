using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class DisconnectDeviceMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public DisconnectDeviceMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
