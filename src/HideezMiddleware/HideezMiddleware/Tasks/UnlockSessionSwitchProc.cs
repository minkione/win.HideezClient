using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Tasks
{
    //todo - doesn't need to be IDisposable if move SubscribeToEvents into Run() method
    class UnlockSessionSwitchProc : IDisposable
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TapConnectionProcessor _tapProcessor;
        readonly RfidConnectionProcessor _rfidProcessor;
        readonly ProximityConnectionProcessor _proximityProcessor;

        readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        readonly string _flowId;

        public WorkstationUnlockResult FlowUnlockResult { get; private set; } = null; // Set on connectionFlow.UnlockAttempt
        public SessionSwitchSubject UnlockMethod { get; private set; } = SessionSwitchSubject.NonHideez; // Changed on connectionFlow.UnlockAttempt
        public bool FlowFinished { get; private set; } // Set on connectionFlow.Finished

        public DateTime SessionSwitchEventTime { get; private set; } // Set on SessionSwitchMonitor.SessionSwitch
        public bool SessionSwitched { get; private set; } // Set on SessionSwitchMonitor.SessionSwitch

        public UnlockSessionSwitchProc(
            string flowId,
            ConnectionFlowProcessor connectionFlowProcessor,
            TapConnectionProcessor tapProcessor,
            RfidConnectionProcessor rfidProcessor,
            ProximityConnectionProcessor proximityProcessor)
        {
            _flowId = flowId;
            _connectionFlowProcessor = connectionFlowProcessor;
            _tapProcessor = tapProcessor;
            _rfidProcessor = rfidProcessor;
            _proximityProcessor = proximityProcessor;

            SubscribeToEvents();
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
            _tapProcessor.WorkstationUnlockPerformed += TapProcessor_WorkstationUnlockPerformed;
            _rfidProcessor.WorkstationUnlockPerformed += RfidProcessor_WorkstationUnlockPerformed;
            _proximityProcessor.WorkstationUnlockPerformed += ProximityProcessor_WorkstationUnlockPerformed;
        }

        void UnsubscribeFromEvents()
        {
            _connectionFlowProcessor.Finished -= ConnectionFlowProcessor_Finished;
            _tapProcessor.WorkstationUnlockPerformed -= TapProcessor_WorkstationUnlockPerformed;
            _rfidProcessor.WorkstationUnlockPerformed -= RfidProcessor_WorkstationUnlockPerformed;
            _proximityProcessor.WorkstationUnlockPerformed -= ProximityProcessor_WorkstationUnlockPerformed;
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

        void TapProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.Dongle;

                if (!FlowUnlockResult.IsSuccessful)
                    _tcs.TrySetResult(new object());
            }
        }

        void RfidProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.RFID;

                if (!FlowUnlockResult.IsSuccessful)
                    _tcs.TrySetResult(new object());
            }
        }

        void ProximityProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.Proximity;

                if (!FlowUnlockResult.IsSuccessful)
                    _tcs.TrySetResult(new object());
            }
        }

    }
}
