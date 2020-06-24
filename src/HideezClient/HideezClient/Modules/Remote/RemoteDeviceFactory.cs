using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Remote;
using HideezClient.Modules.Log;
using HideezClient.Modules.Remote;
using HideezClient.Modules.ServiceProxy;
using HideezMiddleware;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    class RemoteDeviceFactory : IRemoteDeviceFactory
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(RemoteDeviceFactory));
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public RemoteDeviceFactory(IServiceProxy serviceProxy, IMessenger messenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
        }

        public async Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo)
        {
            _log.WriteLine($"({serialNo}) Creating remote vault on channel:{channelNo}");
            var connectionId = await _serviceProxy.GetService().EstablishRemoteDeviceConnectionAsync(serialNo, channelNo);

            var remoteCommands = new RemoteDeviceCommands(_serviceProxy);
            var remoteEvents = new RemoteDeviceEvents(_messenger);

            var device = new RemoteDevice(connectionId, channelNo, remoteCommands, remoteEvents, SdkConfig.DefaultRemoteCommandTimeout, new NLogWrapper());

            remoteCommands.RemoteDevice = device;
            remoteEvents.RemoteDevice = device;

            _log.WriteLine($"({serialNo}) Created remote vault with id: ({device.Id})");

            return device;
        }
    }
}
