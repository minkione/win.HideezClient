using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceProximityChangedMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public double Proximity { get; }

        public DeviceProximityChangedMessage(string deviceId, double proximity)
        {
            DeviceId = deviceId;
            Proximity = proximity;
        }
    }
}
