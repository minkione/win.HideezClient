using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class EstablishRemoteDeviceConnectionMessageReply : PubSubMessageBase
    {
        public string RemoteDeviceId { get; set; }
        public string PipeName { get; set; }
        public string DeviceName { get; set; }
        public string DeviceMac { get; set; }

        public EstablishRemoteDeviceConnectionMessageReply(string remoteDeviceId, string connectionId, string deviceName, string deviceMac)
        {
            RemoteDeviceId = remoteDeviceId;
            PipeName = connectionId;
            DeviceName = deviceName;
            DeviceMac = deviceMac;
        }
    }
}
