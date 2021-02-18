using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages.RemoteDevice
{
    public sealed class RemoteConnection_DeviceIsBusyMessage : PubSubMessageBase
    {
        public string DeviceId { get; set; }

        public RemoteConnection_DeviceIsBusyMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
