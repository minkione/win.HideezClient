using HideezMiddleware.IPC.Messages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class RemoteDevicePubSubManager
    {
        readonly IMetaPubSub _messenger;

        public IMetaPubSub RemoteConnectionPubSub { get; private set; }
        public string PipeName { get; private set; }

        public RemoteDevicePubSubManager(IMetaPubSub messenger)
        {
            RemoteConnectionPubSub = new MetaPubSub(new MetaPubSubLogger(new NLogWrapper()));
            PipeName = "HideezRemoteDevicePipe_" + Guid.NewGuid().ToString();
            _messenger = messenger;

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
        }

        async Task OnRemoteClientDisconnected(RemoteClientDisconnectedEvent arg)
        {
            await _messenger.Publish(new RemoteConnection_ClientDisconnectedMessage(RemoteConnectionPubSub));
        }
    }
}
