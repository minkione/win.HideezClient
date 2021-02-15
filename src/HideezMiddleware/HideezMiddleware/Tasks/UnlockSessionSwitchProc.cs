using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection.Workflow.ConnectionFlow;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Tasks
{
    //todo - doesn't need to be IDisposable if move SubscribeToEvents into Run() method
    class UnlockSessionSwitchProc : IDisposable
    {
        readonly ConnectionFlowProcessorBase _connectionFlowProcessor;
        readonly IMetaPubSub _messenger;

        readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        readonly string _flowId;

        public WorkstationUnlockResult FlowUnlockResult { get; private set; } = null; // Set on connectionFlow.UnlockAttempt
        public SessionSwitchSubject UnlockMethod { get; private set; } = SessionSwitchSubject.NonHideez; // Changed on connectionFlow.UnlockAttempt
        public bool FlowFinished { get; private set; } // Set on connectionFlow.Finished

        public DateTime SessionSwitchEventTime { get; private set; } // Set on SessionSwitchMonitor.SessionSwitch
        public bool SessionSwitched { get; private set; } // Set on SessionSwitchMonitor.SessionSwitch

        public UnlockSessionSwitchProc(
            string flowId,
            ConnectionFlowProcessorBase connectionFlowProcessor,
            IMetaPubSub messenger)
        {
            _flowId = flowId;
            _connectionFlowProcessor = connectionFlowProcessor;

            _messenger = messenger;

            SubscribeToEvents();
        }
        public async Task Run(int timeout)
        {
            try
            {
                // Cancel if unlock failed or flow finished
                if (FlowUnlockResult?.IsSuccessful == false || FlowFinished)
                    return;
                else
                    await _tcs.Task.TimeoutAfter(timeout);
            }
            catch (TimeoutException)
            {
            }
            finally
            {
                UnsubscribeFromEvents();
            }

        }

        void SubscribeToEvents()
        {
            _connectionFlowProcessor.Finished += ConnectionFlowProcessor_Finished;
            _messenger.Subscribe<WorkstationUnlockPerformedMessage>(OnWorkstationUnlockPerformed);
        }

        void UnsubscribeFromEvents()
        {
            _connectionFlowProcessor.Finished -= ConnectionFlowProcessor_Finished;
            _messenger.Unsubscribe<WorkstationUnlockPerformedMessage>(OnWorkstationUnlockPerformed);
        }

        void ConnectionFlowProcessor_Finished(object sender, string e)
        {
            if (e == _flowId)
            {
                FlowFinished = true;
                UnsubscribeFromEvents();

                _tcs.TrySetResult(new object());
            }
        }

        private Task OnWorkstationUnlockPerformed(WorkstationUnlockPerformedMessage msg)
        {
            if (msg.FlowId == _flowId)
            {
                FlowUnlockResult = new WorkstationUnlockResult
                {
                    FlowId = msg.FlowId,
                    IsSuccessful = msg.IsSuccessful,
                    DeviceMac = msg.Mac,
                    AccountName = msg.AccountName,
                    AccountLogin = msg.AccountLogin,
                }; 
                UnlockMethod = msg.UnlockMethod;

                if (!FlowUnlockResult.IsSuccessful)
                    _tcs.TrySetResult(new object());
            }

            return Task.CompletedTask;
        }

        #region IDisposable Support
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    UnsubscribeFromEvents();
                }

                disposed = true;
            }
        }

        ~UnlockSessionSwitchProc()
        {
            Dispose(false);
        }
        #endregion
    }
}
