using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceBatteryChangedMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public int Battery { get; }

        public DeviceBatteryChangedMessage(string deviceId, int battery)
        {
            DeviceId = deviceId;
            Battery = battery;
        }
    }
}
