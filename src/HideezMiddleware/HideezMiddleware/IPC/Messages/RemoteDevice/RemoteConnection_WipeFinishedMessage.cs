using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages.RemoteDevice
{
    public sealed class RemoteConnection_WipeFinishedMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public FwWipeStatus WipeStatus { get; set; }

        public RemoteConnection_WipeFinishedMessage(string deviceId, FwWipeStatus wipeStatus)
        {
            DeviceId = deviceId;
            WipeStatus = wipeStatus;
        }
    }
}
