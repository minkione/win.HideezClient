using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Remote;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using HideezMiddleware.IPC.IncommingMessages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using System;
using System.ServiceModel;
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

        public async Task<GetRootKeyReply> SendGetRootKeyCommand()
        {
            var message = new GetRootKeyCommand();
            var response = await _metaMessenger.ProcessOnServer<RemoteConnection_RemoteCommandMessageReply>(new RemoteConnection_GetRootKeyMessage(_connectionId, message.Data), SdkConfig.GetRootKeyCommandTimeout);
            return new GetRootKeyReply(response.Data);
        }

        public async Task SendResetCommand(byte channelNo)
        {
            await _metaMessenger.PublishOnServer(new RemoteConnection_ResetChannelMessage(_connectionId, channelNo));
        }

        public async Task<VerifyReply> SendVerifyCommand(byte[] pubKeyH, byte[] nonceH, byte verifyChannelNo)
        {
            var response = await _metaMessenger.ProcessOnServer<RemoteConnection_VerifyCommandMessageReply>(new RemoteConnection_VerifyCommandMessage(_connectionId, pubKeyH, nonceH, verifyChannelNo), SdkConfig.DeviceInitializationTimeout);
            return new VerifyReply(response.Data);
        }
    }
}
