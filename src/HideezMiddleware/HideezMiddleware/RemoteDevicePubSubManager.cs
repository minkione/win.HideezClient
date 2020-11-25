using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PipeDevice;
using HideezMiddleware.IPC.IncommingMessages.RemoteDevice;
using HideezMiddleware.IPC.Messages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using Newtonsoft.Json;
using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    /// <summary>
    /// Class for creating a direct MetaPubSub for each remote pipe device.
    /// </summary>
    public class RemoteDevicePubSubManager: Logger
    {
        readonly IMetaPubSub _messenger;
        private readonly PipeRemoteDeviceProxy _pipeDevice;

        /// <summary>
        /// Direct MetaPubSub for each pipe device.
        /// </summary>
        public IMetaPubSub RemoteConnectionPubSub { get; private set; }

        public string PipeName { get; private set; }

        public RemoteDevicePubSubManager(IMetaPubSub messenger, PipeRemoteDeviceProxy pipeDevice, ILog log)
            :base(nameof(RemoteDevicePubSubManager), log)
        {
            RemoteConnectionPubSub = new MetaPubSub(new MetaPubSubLogger(new NLogWrapper()));
            PipeName = "HideezRemoteDevicePipe_" + Guid.NewGuid().ToString();
            _messenger = messenger;
            _pipeDevice = pipeDevice;

            InitializePubSub();
        }

        void InitializePubSub()
        {
            RemoteConnectionPubSub.StartServer(PipeName, () =>
            {
                var pipeSecurity = new PipeSecurity();
                pipeSecurity.AddAccessRule(new PipeAccessRule(
                    new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                    PipeAccessRights.FullControl,
                    AccessControlType.Allow));

                var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 32,
                    PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);

                return pipe;
            });

            RemoteConnectionPubSub.Subscribe<RemoteClientDisconnectedEvent>(OnRemoteClientDisconnected);
            RemoteConnectionPubSub.Subscribe<RemoteConnection_RemoteCommandMessage>(RemoteConnection_RemoteCommandAsync);
            RemoteConnectionPubSub.Subscribe<RemoteConnection_GetRootKeyMessage>(RemoteConnection_GetRootKeyAsync);
            RemoteConnectionPubSub.Subscribe<RemoteConnection_ControlRemoteCommandMessage>(RemoteConnection_ControlRemoteCommandAsync);
            RemoteConnectionPubSub.Subscribe<RemoteConnection_VerifyCommandMessage>(RemoteConnection_VerifyCommandAsync);
            RemoteConnectionPubSub.Subscribe<RemoteConnection_ResetChannelMessage>(RemoteConnection_ResetChannelAsync);
        }

        private async Task RemoteConnection_ControlRemoteCommandAsync(RemoteConnection_ControlRemoteCommandMessage arg)
        {
            try
            {
                if (_pipeDevice == null || _pipeDevice.Id != arg.ConnectionId)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, arg.ConnectionId);

                var data = JsonConvert.DeserializeObject<ControlRequest>(arg.Data);
                if (data != null)
                    await _pipeDevice.SendControlRequest(data);
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RemoteConnection_RemoteCommandAsync(RemoteConnection_RemoteCommandMessage args)
        {
            try
            {
                if (_pipeDevice == null || _pipeDevice.Id != args.ConnectionId)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                byte[] response = null;
                var data = JsonConvert.DeserializeObject<EncryptedRequest>(args.Data);

                if (data != null)
                    response = await _pipeDevice.SendRequestAndWaitResponseAsync(data, null);

                if (response != null)
                    await RemoteConnectionPubSub.Publish(new RemoteConnection_RemoteCommandMessageReply(response));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RemoteConnection_VerifyCommandAsync(RemoteConnection_VerifyCommandMessage args)
        {
            try
            {
                if (_pipeDevice == null || _pipeDevice.Id != args.ConnectionId)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                if (args.VerifyChannelNo < 2 || args.VerifyChannelNo > 7)
                    throw new HideezException(HideezErrorCode.InvalidChannelNo, args.VerifyChannelNo);

                var response = await _pipeDevice.Device.SendVerifyCommand(args.PubKeyH, args.NonceH, args.VerifyChannelNo);

                await RemoteConnectionPubSub.Publish(new RemoteConnection_VerifyCommandMessageReply(response.Result));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RemoteConnection_ResetChannelAsync(RemoteConnection_ResetChannelMessage args)
        {
            try
            {
                if (_pipeDevice == null || _pipeDevice.Id != args.ConnectionId)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                await _pipeDevice.Device.SendResetCommand(args.ChannelNo);
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }
        
        async Task RemoteConnection_GetRootKeyAsync(RemoteConnection_GetRootKeyMessage args)
        {
            try
            {
                if (_pipeDevice == null || _pipeDevice.Id != args.ConnectionId)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                var response = await _pipeDevice.Device.SendGetRootKeyCommand();

                if(response!= null)
                    await RemoteConnectionPubSub.Publish(new RemoteConnection_RemoteCommandMessageReply(response.Result));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task OnRemoteClientDisconnected(RemoteClientDisconnectedEvent arg)
        {
            await _messenger.Publish(new RemoteDeviceDisconnectedMessage(RemoteConnectionPubSub));
        }

        void Error(Exception ex, string message = "")
        {
            _log?.WriteLine(message, ex);
        }
    }
}
