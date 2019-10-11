using Hideez.SDK.Communication;
using HideezMiddleware.DeviceConnection;
using System;

namespace HideezMiddleware.Tasks
{
    class UnlockSessionSwitchProc : IDisposable
    {
        const int UNLOCK_EVENT_TIMEOUT = 10_000;

        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TapConnectionProcessor _tapProcessor;
        readonly RfidConnectionProcessor _rfidProcessor;
        readonly ProximityConnectionProcessor _proximityProcessor;

        string _flowId;

        public WorkstationUnlockResult FlowUnlockResult { get; private set; } = null; // Set on connectionFlow.UnlockAttempt
        public SessionSwitchSubject UnlockMethod { get; private set; } = SessionSwitchSubject.NonHideez; // Changed on connectionFlow.UnlockAttempt
        bool _flowFinished; // Set on connectionFlow.Finished

        DateTime _sessionSwitchEventTime; // Set on SessionSwitchMonitor.SessionSwitch
        bool _sessionSwitched; // Set on SessionSwitchMonitor.SessionSwitch

        Action<UnlockSessionSwitchProc> _onProcCancelled = null;
        Action<UnlockSessionSwitchProc> _onProcFinished = null;

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
                    UnsubscriveFromEvents();
                }

                disposed = true;
            }
        }

        ~UnlockSessionSwitchProc()
        {
            Dispose(false);
        }
        #endregion

        // Todo: add cancellation token and timeout
        public void Run(Action<UnlockSessionSwitchProc> onProcCancelled, Action<UnlockSessionSwitchProc> onProcFinished)
        {
            _onProcCancelled = onProcCancelled;
            _onProcFinished = onProcFinished;

            SubscribeToEvents();
        }

        void SubscribeToEvents()
        {
            _connectionFlowProcessor.Finished += ConnectionFlowProcessor_Finished;
            _tapProcessor.WorkstationUnlockPerformed += TapProcessor_WorkstationUnlockPerformed;
            _rfidProcessor.WorkstationUnlockPerformed += RfidProcessor_WorkstationUnlockPerformed;
            _proximityProcessor.WorkstationUnlockPerformed += ProximityProcessor_WorkstationUnlockPerformed;
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }

        void UnsubscriveFromEvents()
        {
            _connectionFlowProcessor.Finished -= ConnectionFlowProcessor_Finished;
            _tapProcessor.WorkstationUnlockPerformed -= TapProcessor_WorkstationUnlockPerformed;
            _rfidProcessor.WorkstationUnlockPerformed -= RfidProcessor_WorkstationUnlockPerformed;
            _proximityProcessor.WorkstationUnlockPerformed -= ProximityProcessor_WorkstationUnlockPerformed;
            SessionSwitchMonitor.SessionSwitch -= SessionSwitchMonitor_SessionSwitch;
        }

        void ConnectionFlowProcessor_Finished(object sender, string e)
        {
            if (e == _flowId)
            {
                _flowFinished = true;
            }
        }

        void TapProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.Dongle;
            }
        }

        void RfidProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.RFID;
            }
        }

        void ProximityProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.Proximity;
            }
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, Microsoft.Win32.SessionSwitchReason reason)
        {
            _sessionSwitchEventTime = DateTime.UtcNow;
            _sessionSwitched = true;
        }

        // Todo: Call CancelProc and FinishProc when certain events occur
        void CancelProc()
        {
            UnsubscriveFromEvents();

            _onProcCancelled?.Invoke(this);
        }

        void FinishProc()
        {
            UnsubscriveFromEvents();

            _onProcFinished?.Invoke(this);
        }
    }
}
