using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.ReconnectManager
{
    class ReconnectProc
    {
        readonly IDevice _device;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public ReconnectProc(IDevice device, ConnectionFlowProcessor connectionFlowProcessor)
        {
            _device = device;
            _connectionFlowProcessor = connectionFlowProcessor;
        }

        public async Task<bool> Run(int timeout, CancellationToken ct)
        {
            try
            {
                _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;
                await _connectionFlowProcessor.ConnectAndUnlock(_device.Mac, null);
                
                return await _tcs.Task.TimeoutAfter(timeout, ct);
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
                _tcs.TrySetResult(true);
        }
    }
}
