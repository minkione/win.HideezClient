using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using HideezSafe.HideezServiceReference;
using HideezSafe.Modules.ServiceProxy;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HideezSafe.Modules.Remote
{
    class RemoteDeviceCommands : IRemoteCommands
    {
        readonly IServiceProxy _serviceProxy;

        public RemoteDeviceCommands(IServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;
        }

        // Temporary duct tape, until IRemoteDeviceConnection is refactored
        public RemoteDevice RemoteDevice { get; set; }

        public async Task ResetChannel()
        {
            try
            {
                await _serviceProxy.GetService().RemoteConnection_ResetChannelAsync(RemoteDevice.DeviceId);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task SendAuthCommand(byte[] data)
        {
            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_AuthCommandAsync(RemoteDevice.DeviceId, data);
                RemoteDevice.OnAuthResponse(response);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }

        }

        public async Task SendRemoteCommand(byte[] data)
        {
            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_RemoteCommandAsync(RemoteDevice.DeviceId, data);
                RemoteDevice.OnCommandResponse(response);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }
        }
    }
}
