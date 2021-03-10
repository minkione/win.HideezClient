using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    internal sealed class WipeFinishedMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public FwWipeStatus Status { get; }

        public WipeFinishedMessage(string deviceId, FwWipeStatus status)
        {
            DeviceId = deviceId;
            Status = status;
        }
    }
}
