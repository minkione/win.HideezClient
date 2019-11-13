using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Remote;
using HideezClient.Modules.Remote;
using HideezClient.Modules.ServiceProxy;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    class RemoteDeviceFactory : Logger, IRemoteDeviceFactory
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public RemoteDeviceFactory(IServiceProxy serviceProxy, IMessenger messenger, ILog log)
            : base(nameof(RemoteDeviceFactory), log)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
        }

        public async Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo)
        {
            WriteLine($"Creating remote device ({serialNo}) on channel:{channelNo}");
            var connectionId = await _serviceProxy.GetService().EstablishRemoteDeviceConnectionAsync(serialNo, channelNo);

            var remoteCommands = new RemoteDeviceCommands(_serviceProxy, _messenger, _log);
            var remoteEvents = new RemoteDeviceEvents(_messenger);

            var device = new RemoteDevice(connectionId, channelNo, remoteCommands, remoteEvents, SdkConfig.DefaultRemoteCommandTimeout, _log);

            remoteCommands.RemoteDevice = device;
            remoteEvents.RemoteDevice = device;

            return device;
        }
    }
}
