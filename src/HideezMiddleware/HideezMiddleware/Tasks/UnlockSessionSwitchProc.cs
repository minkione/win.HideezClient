using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.DeviceConnection.Workflow;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Tasks
{
    internal sealed class UnlockSessionSwitchProc
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TapConnectionProcessor _tapProcessor;
        readonly RfidConnectionProcessor _rfidProcessor;
        readonly ProximityConnectionProcessor _proximityProcessor;
        readonly WinBleAutomaticConnectionProcessor _winBleProcessor;

        readonly TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        readonly string _flowId;

        readonly int _timeout;

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
            ProximityConnectionProcessor proximityProcessor,
            WinBleAutomaticConnectionProcessor winBleProcessor,
            int timeout = 10_000)
        {
            _flowId = flowId;
            _connectionFlowProcessor = connectionFlowProcessor;
            _tapProcessor = tapProcessor;
            _rfidProcessor = rfidProcessor;
            _proximityProcessor = proximityProcessor;
            _winBleProcessor = winBleProcessor;

            _timeout = timeout;
        }

        public async Task Run()
        {
            try
            {
                SubscribeToEvents();
                
                // Cancel if unlock failed or flow finished
                await _tcs.Task.TimeoutAfter(_timeout);
            }
            catch (TimeoutException)
            {
                _tcs.TrySetResult(new object());
            }
            finally
            {
                UnsubscribeFromEvents();
            }
        }

        public async Task WaitFinish()
        {
            try
            {
                await _tcs.Task.TimeoutAfter(_timeout);
            }
            catch (TimeoutException)
            {
            }
        }

        void SubscribeToEvents()
        {
            _connectionFlowProcessor.Finished += ConnectionFlowProcessor_Finished;
            _tapProcessor.WorkstationUnlockPerformed += TapProcessor_WorkstationUnlockPerformed;
            _rfidProcessor.WorkstationUnlockPerformed += RfidProcessor_WorkstationUnlockPerformed;
            _proximityProcessor.WorkstationUnlockPerformed += ProximityProcessor_WorkstationUnlockPerformed;
            _winBleProcessor.WorkstationUnlockPerformed += WinBleProcessor_WorkstationUnlockPerformed;
        }

        void UnsubscribeFromEvents()
        {
            _connectionFlowProcessor.Finished -= ConnectionFlowProcessor_Finished;
            _tapProcessor.WorkstationUnlockPerformed -= TapProcessor_WorkstationUnlockPerformed;
            _rfidProcessor.WorkstationUnlockPerformed -= RfidProcessor_WorkstationUnlockPerformed;
            _proximityProcessor.WorkstationUnlockPerformed -= ProximityProcessor_WorkstationUnlockPerformed;
            _winBleProcessor.WorkstationUnlockPerformed -= WinBleProcessor_WorkstationUnlockPerformed;
        }

        void ConnectionFlowProcessor_Finished(object sender, string e)
        {
            if (e == _flowId)
            {
                FlowFinished = true;

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

        void WinBleProcessor_WorkstationUnlockPerformed(object sender, WorkstationUnlockResult e)
        {
            if (e.FlowId == _flowId)
            {
                FlowUnlockResult = e;
                UnlockMethod = SessionSwitchSubject.WinBle;

                if (!FlowUnlockResult.IsSuccessful)
                    _tcs.TrySetResult(new object());
            }
        }

    }
}
