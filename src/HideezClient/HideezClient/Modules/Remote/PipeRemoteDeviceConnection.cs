using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.Interfaces;
using HideezMiddleware.IPC.IncommingMessages.RemoteDevice;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules.Remote
{
    public class PipeRemoteDeviceConnection: IConnectionController
    {
        private readonly IMetaPubSub _metaPubSub;

        public string Id { get; }

        public string Name { get; }

        public ConnectionState State { get; }

        public string Mac { get; }

        public IConnection Connection => throw new NotImplementedException();

        public event EventHandler<MessageBuffer> ResponseReceived;
        public event EventHandler OperationCancelled;
        public event EventHandler<byte[]> DeviceStateChanged;
        public event EventHandler DeviceIsBusy;
        public event EventHandler<FwWipeStatus> WipeFinished;
        public event EventHandler ConnectionStateChanged;

        public PipeRemoteDeviceConnection(IMetaPubSub metaPubSub, string id, string mac, string name, bool isConnected)
        {
            _metaPubSub = metaPubSub;

            Id = id;
            Name = name;
            Mac = mac;

            if (isConnected)
                State = ConnectionState.Connected;
            else State = ConnectionState.NotConnected;

            _metaPubSub.TrySubscribeOnServer<RemoteConnection_DeviceStateChangedMessage>(OnDeviceStateChanged);
        }

        private Task OnDeviceStateChanged(RemoteConnection_DeviceStateChangedMessage arg)
        {
            DeviceStateChanged?.Invoke(this, arg.State);
            return Task.CompletedTask;
        }

        public bool IsBoot()
        {
            return false;
        }

        public async Task SendRequestAsync(EncryptedRequest request)
        {
            var response = await _metaPubSub.ProcessOnServer<RemoteConnection_RemoteCommandMessageReply>(new RemoteConnection_RemoteCommandMessage(Id, request), 10_000);
            if (response != null)
            {
                ResponseReceived?.Invoke(this, new MessageBuffer(response.Data, request.Buffer.ChannelNo));
            }
        }

        public async Task SendRequestAsync(ControlRequest request)
        {
            await _metaPubSub.PublishOnServer(new RemoteConnection_ControlRemoteCommandMessage(Id, request));
        }
    }
}
