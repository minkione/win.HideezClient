using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Remote;
using HideezSafe.Modules.Remote;
using HideezSafe.Modules.ServiceProxy;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class RemoteDeviceFactory : IRemoteDeviceFactory
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public RemoteDeviceFactory(IServiceProxy serviceProxy, IMessenger messenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
        }

        public async Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo)
        {
            var connectionId = await _serviceProxy.GetService().EstablishRemoteDeviceConnectionAsync(serialNo, channelNo);

            var remoteCommands = new RemoteDeviceCommands(_serviceProxy);
            var remoteEvents = new RemoteDeviceEvents(_messenger);

            var device = new RemoteDevice(connectionId, remoteCommands, remoteEvents);

            remoteCommands.RemoteDevice = device;
            remoteEvents.RemoteDevice = device;

            return device;
        }
    }
}
