using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection.Workflow;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.ReconnectManager
{
    class ReconnectProc
    {
        readonly IDevice _device;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        bool _isReconnectSuccessful = false;

        public ReconnectProc(IDevice device, ConnectionFlowProcessor connectionFlowProcessor)
        {
            _device = device;
            _connectionFlowProcessor = connectionFlowProcessor;
        }

        public async Task<bool> Run()
        {
            try
            {
                _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;
                await _connectionFlowProcessor.Connect(_device.Mac);
                return _isReconnectSuccessful;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _connectionFlowProcessor.DeviceFinishedMainFlow -= ConnectionFlowProcessor_DeviceFinishedMainFlow;
            }

        }

        void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice e)
        {
            if (e.Id == _device.Id)
                _isReconnectSuccessful = true;
        }
    }
}
