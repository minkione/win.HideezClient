using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class EstablishRemoteDeviceConnectionMessage : PubSubMessageBase
    {
        public string SerialNo { get; set; }
        public byte ChannelNo { get; set; }

        public EstablishRemoteDeviceConnectionMessage(string serialNo, byte channelNo)
        {
            SerialNo = serialNo;
            ChannelNo = channelNo;
            ResponseTimeout = SdkConfig.ConnectDeviceTimeout;
        }
    }
}
