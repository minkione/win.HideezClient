using Hideez.SDK.Communication;
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
    class RemoteDeviceCommands : IRemoteCommands
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(RemoteDeviceCommands));
        readonly IServiceProxy _serviceProxy;
        readonly IMetaPubSub _metaMessenger;

        public RemoteDeviceCommands(IServiceProxy serviceProxy, IMetaPubSub metaMessenger)
        {
            _serviceProxy = serviceProxy;
            _metaMessenger = metaMessenger;
        }

        // Todo: fix cyclic dependency between RemoteDevice and RemoteCommands/RemoteEvents
        public RemoteDevice RemoteDevice { get; set; }

        public async Task ResetChannel()
        {
            await _metaMessenger.PublishOnServer(new RemoteConnection_ResetChannelMessage(RemoteDevice.Id));
        }

        public async Task SendVerifyCommand(byte[] data)
        {
            string error = null;

            try
            {
                var response = await _metaMessenger.ProcessOnServer<RemoteConnection_VerifyCommandMessageReply>(new RemoteConnection_VerifyCommandMessage(RemoteDevice.Id, data), 0);
                RemoteDevice.OnVerifyResponse(response.Data, null);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _log.WriteLine(ex);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                try
                {
                    RemoteDevice.OnVerifyResponse(null, error);
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex);
                }
            }

        }

        public async Task SendRemoteCommand(byte[] data)
        {
            string error = null;

            try
            {
                var response = await _metaMessenger.ProcessOnServer<RemoteConnection_RemoteCommandMessageReply>(new RemoteConnection_RemoteCommandMessage(RemoteDevice.Id, data), 0);
                RemoteDevice.OnCommandResponse(response.Data, null);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _log.WriteLine(ex);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                try
                {
                    RemoteDevice.OnVerifyResponse(null, error);
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex);
                }
            }
        }
    }
}
