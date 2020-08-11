using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Remote;
using HideezClient.Modules.Log;
using HideezClient.Modules.Remote;
using HideezClient.Modules.ServiceProxy;
using HideezMiddleware;
using HideezMiddleware.IPC.IncommingMessages;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    class RemoteDeviceFactory : IRemoteDeviceFactory
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(RemoteDeviceFactory));
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;
        readonly IMetaPubSub _metaMessenger;

        public RemoteDeviceFactory(IServiceProxy serviceProxy, IMessenger messenger, IMetaPubSub metaMessenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
            _metaMessenger = metaMessenger;
        }

        public async Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo)
        {
            _log.WriteLine($"({serialNo}) Creating remote vault on channel:{channelNo}");
            var reply = await _metaMessenger.ProcessOnServer<EstablishRemoteDeviceConnectionMessageReply>(new EstablishRemoteDeviceConnectionMessage(serialNo, channelNo), 0);
            var connectionId = reply.RemoveDeviceId;

            var remoteCommands = new RemoteDeviceCommands(_serviceProxy, _metaMessenger);
            var remoteEvents = new RemoteDeviceEvents(_metaMessenger);

            var device = new RemoteDevice(connectionId, channelNo, remoteCommands, remoteEvents, SdkConfig.DefaultRemoteCommandTimeout, new NLogWrapper());

            remoteCommands.RemoteDevice = device;
            remoteEvents.RemoteDevice = device;

            _log.WriteLine($"({serialNo}) Created remote vault with id: ({device.Id})");

            return device;
        }
    }
}
