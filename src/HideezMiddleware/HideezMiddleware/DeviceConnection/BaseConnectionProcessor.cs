using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    public abstract class BaseConnectionProcessor : Logger, IConnectionProcessor
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;

        public event EventHandler<WorkstationUnlockResult> WorkstationUnlockPerformed;

        public BaseConnectionProcessor(ConnectionFlowProcessor connectionFlowProcessor, string logSource, ILog log)
            : base(logSource, log)
        {
            _connectionFlowProcessor = connectionFlowProcessor ?? throw new ArgumentNullException(nameof(connectionFlowProcessor));
        }

        public abstract void Start();

        public abstract void Stop();

        protected async Task ConnectAndUnlockByConnectionId(ConnectionId connectionId)
        {
            await _connectionFlowProcessor.ConnectAndUnlock(connectionId, OnUnlockAttempt);
        }

        void OnUnlockAttempt(WorkstationUnlockResult result)
        {
            if (result.IsSuccessful)
                WorkstationUnlockPerformed?.Invoke(this, result);
        }
    }
}
