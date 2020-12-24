using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezMiddleware.IPC.IncommingMessages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;

namespace HideezClient.Modules.Remote
{
    class RemoteDeviceCommands : IDeviceCommands
    {
        readonly string _connectionId;
        readonly IMetaPubSub _metaMessenger;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(RemoteDeviceCommands));

        public RemoteDeviceCommands(IMetaPubSub metaMessenger, string connectionId)
        {
            _metaMessenger = metaMessenger;
            _connectionId = connectionId;
        }

        public async Task<DeviceCommandReplyResult> GetRootKey()
        {
            var response = await _metaMessenger.ProcessOnServer<RemoteConnection_DeviceCommandMessageReply>(new RemoteConnection_GetRootKeyMessage(_connectionId));
            return response.Data;
        }

        public async Task ResetEncryption(byte channelNo)
        {
            await _metaMessenger.PublishOnServer(new RemoteConnection_ResetChannelMessage(_connectionId, channelNo));
        }

        public async Task<DeviceCommandReplyResult> VerifyEncryption(byte[] pubKeyH, byte[] nonceH, byte verifyChannelNo)
        {
            var response = await _metaMessenger.ProcessOnServer<RemoteConnection_DeviceCommandMessageReply>(new RemoteConnection_VerifyCommandMessage(_connectionId, pubKeyH, nonceH, verifyChannelNo));
            return response.Data;
        }
    }
}
