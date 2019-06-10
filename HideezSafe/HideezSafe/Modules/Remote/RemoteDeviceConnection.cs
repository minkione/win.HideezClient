using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using HideezSafe.Modules.ServiceProxy;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    class RemoteDeviceConnection : IRemoteDeviceConnection
    {
        readonly IServiceProxy _serviceProxy;

        public RemoteDeviceConnection(IServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;
        }

        // Temporary duct tape, until IRemoteDeviceConnection is refactored
        public RemoteDevice RemoteDevice { get; set; }

        public async Task ResetChannel()
        {
            await _serviceProxy.GetService().RemoteConnection_ResetChannelAsync(RemoteDevice.DeviceId);
        }

        public async Task SendAuthCommand(byte[] data)
        {
            var response = await _serviceProxy.GetService().RemoteConnection_AuthCommandAsync(RemoteDevice.DeviceId, data);
            RemoteDevice.OnAuthResponse(response);
        }

        public async Task SendRemoteCommand(byte[] data)
        {
            var response = await _serviceProxy.GetService().RemoteConnection_RemoteCommandAsync(RemoteDevice.DeviceId, data);
            RemoteDevice.OnCommandResponse(response);
        }
    }
}
