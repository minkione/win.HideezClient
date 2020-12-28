using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class EstablishRemoteDeviceConnectionMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }
        public byte ChannelNo { get; set; }

        public EstablishRemoteDeviceConnectionMessage(string connectionId, byte channelNo)
        {
            ConnectionId = connectionId;
            ChannelNo = channelNo;
            ResponseTimeout = SdkConfig.ConnectDeviceTimeout;
        }
    }
}
