using Hideez.SDK.Communication.Log;
using HideezMiddleware.IPC.Messages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
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
    public class RemoteDevicePubSubManager : Logger
    {
        readonly IMetaPubSub _messenger;

        /// <summary>
        /// Direct MetaPubSub for each pipe device.
        /// </summary>
        public IMetaPubSub RemoteConnectionPubSub { get; private set; }

        public string PipeName { get; private set; }

        public RemoteDevicePubSubManager(IMetaPubSub messenger, ILog log)
            : base(nameof(RemoteDevicePubSubManager), log)
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
                try
                {
                    WriteLine("Custom pipe config started");
                    var pipeSecurity = new PipeSecurity();
                    pipeSecurity.AddAccessRule(new PipeAccessRule(
                        new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                        PipeAccessRights.FullControl,
                        AccessControlType.Allow));

                    var pipe = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 32,
                        PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);

                    WriteLine("Custom pipe config successful");
                    return pipe;
                }
                catch (Exception ex)
                {
                    WriteLine("Custom pipe config failed.", ex);
                    return null;
                }
            });

            RemoteConnectionPubSub.Subscribe<RemoteClientDisconnectedEvent>(OnRemoteClientDisconnected);
        }

        async Task OnRemoteClientDisconnected(RemoteClientDisconnectedEvent arg)
        {
            await _messenger.Publish(new RemoteDeviceDisconnectedMessage(RemoteConnectionPubSub));
        }
    }
}
