using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages.RemoteDevice
{
    public sealed class RemoteConnection_VerifyCommandMessage : PubSubMessageBase
    {
        public string ConnectionId { get; set; }

        public byte[] PubKeyH { get; set; }

        public byte[] NonceH { get; }

        public byte VerifyChannelNo { get; }

        public RemoteConnection_VerifyCommandMessage(string connectionId, byte[] pubKeyH, byte[] nonceH, byte verifyChannelNo)
        {
            ConnectionId = connectionId;
            PubKeyH = pubKeyH;
            NonceH = nonceH;
            VerifyChannelNo = verifyChannelNo;
            ResponseTimeout = SdkConfig.DeviceInitializationTimeout;
        }
    }
}
