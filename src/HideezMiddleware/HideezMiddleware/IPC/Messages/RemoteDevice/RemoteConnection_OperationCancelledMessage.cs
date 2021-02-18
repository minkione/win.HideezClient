using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages.RemoteDevice
{
    public sealed class RemoteConnection_OperationCancelledMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public RemoteConnection_OperationCancelledMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
