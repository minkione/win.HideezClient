using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Remote;
using HideezClient.Modules.Remote;
using HideezClient.Modules.ServiceProxy;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    class RemoteDeviceFactory : IRemoteDeviceFactory
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;
        readonly ILog _log;

        public RemoteDeviceFactory(IServiceProxy serviceProxy, IMessenger messenger, ILog log)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
            _log = log;
        }

        public async Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo)
        {
            var connectionId = await _serviceProxy.GetService().EstablishRemoteDeviceConnectionAsync(serialNo, channelNo);

            var remoteCommands = new RemoteDeviceCommands(_serviceProxy);
            var remoteEvents = new RemoteDeviceEvents(_messenger);

            var device = new RemoteDevice(connectionId, remoteCommands, remoteEvents, _log);

            remoteCommands.RemoteDevice = device;
            remoteEvents.RemoteDevice = device;

            return device;
        }
    }
}
