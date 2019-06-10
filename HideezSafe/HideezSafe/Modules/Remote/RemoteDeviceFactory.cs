using Hideez.SDK.Communication.Remote;
using HideezSafe.Modules.ServiceProxy;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class RemoteDeviceFactory : IRemoteDeviceFactory
    {
        readonly IServiceProxy _serviceProxy;

        public RemoteDeviceFactory(IServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;
        }

        public async Task<RemoteDevice> CreateRemoteDevice(string mac, byte channelNo)
        {
            var connectionId = await _serviceProxy.GetService().EstablishRemoteDeviceConnectionAsync(mac, channelNo);

            var remoteConnection = new RemoteDeviceConnection(_serviceProxy);

            var device = new RemoteDevice(connectionId, remoteConnection);

            remoteConnection.RemoteDevice = device;

            return device;
        }
    }
}
