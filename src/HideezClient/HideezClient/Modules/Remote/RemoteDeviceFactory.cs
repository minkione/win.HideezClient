using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
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
    class RemoteDeviceFactory : Logger, IRemoteDeviceFactory
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger(nameof(RemoteDeviceFactory));
        readonly IServiceProxy _serviceProxy;
        readonly IMetaPubSub _metaMessenger;

        public RemoteDeviceFactory(IServiceProxy serviceProxy, IMetaPubSub metaMessenger, ILog log)
            :base(nameof(RemoteDeviceFactory), log)
        {
            _serviceProxy = serviceProxy;
            _metaMessenger = metaMessenger;
        }

        public async Task<Device> CreateRemoteDeviceAsync(string serialNo, byte channelNo, IMetaPubSub remoteDeviceMessenger)
        {
            _logger.WriteLine($"({serialNo}) Creating remote vault on channel:{channelNo}");
            var reply = await _metaMessenger.ProcessOnServer<EstablishRemoteDeviceConnectionMessageReply>(new EstablishRemoteDeviceConnectionMessage(serialNo, channelNo), SdkConfig.ConnectDeviceTimeout);
            var remoteDeviceId = reply.RemoteDeviceId;

            var remoteCommands = new RemoteDeviceCommands(_serviceProxy, _metaMessenger);
            var remoteEvents = new RemoteDeviceEvents(_metaMessenger);

            var pipeRemoteDeviceConnection = new PipeRemoteDeviceConnection(_metaMessenger, remoteDeviceId);
            var commandQueue = new CommandQueue(pipeRemoteDeviceConnection, _log);
            var device = new Device(commandQueue, channelNo, _log);
            await remoteDeviceMessenger.TryConnectToServer(reply.ConnectionId);

            remoteCommands.RemoteDevice = device;
            remoteEvents.RemoteDevice = device;

            _logger.WriteLine($"({serialNo}) Created remote vault with id: ({device.Id})");

            return device;
        }
    }
}
