using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using HideezClient.HideezServiceReference;
using HideezClient.Modules.ServiceProxy;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HideezClient.Modules.Remote
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

        public async Task SendVerifyCommand(byte[] data)
        {
            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_VerifyCommandAsync(RemoteDevice.DeviceId, data);
                RemoteDevice.OnVerifyResponse(response);
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
