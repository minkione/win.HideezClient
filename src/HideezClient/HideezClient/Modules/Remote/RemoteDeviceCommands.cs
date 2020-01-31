using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Remote;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HideezClient.Modules.Remote
{
    class RemoteDeviceCommands : IRemoteCommands
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(RemoteDeviceCommands));
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public RemoteDeviceCommands(IServiceProxy serviceProxy, IMessenger messenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
        }

        // Todo: fix cyclic dependency between RemoteDevice and RemoteCommands/RemoteEvents
        public RemoteDevice RemoteDevice { get; set; }

        public async Task ResetChannel()
        {
            try
            {
                await _serviceProxy.GetService().RemoteConnection_ResetChannelAsync(RemoteDevice.Id);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                throw ex.InnerException;
            }
        }

        public async Task SendVerifyCommand(byte[] data)
        {
            string error = null;

            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_VerifyCommandAsync(RemoteDevice.Id, data);
                RemoteDevice.OnVerifyResponse(response, null);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                error = ex.Message;
                _log.WriteLine(ex);
                _messenger.Send(new ShowErrorNotificationMessage(error));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _log.WriteLine(ex);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                try
                {
                    RemoteDevice.OnVerifyResponse(null, error);
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex);
                    _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                }
            }

        }

        public async Task SendRemoteCommand(byte[] data)
        {
            string error = null;

            try
            {
                var response = await _serviceProxy.GetService().RemoteConnection_RemoteCommandAsync(RemoteDevice.Id, data);
                RemoteDevice.OnCommandResponse(response, null);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                error = ex.Message;
                _log.WriteLine(ex);
                _messenger.Send(new ShowErrorNotificationMessage(error));
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _log.WriteLine(ex);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                try
                {
                    RemoteDevice.OnVerifyResponse(null, error);
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex);
                    _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                }
            }
        }
    }
}
