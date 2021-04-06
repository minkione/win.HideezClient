using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.DeviceConnection.Workflow;
using HideezMiddleware.Tasks;
using HideezMiddleware.Utils.WorkstationHelper;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class SessionUnlockMethodMonitor : Logger
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TapConnectionProcessor _tapProcessor;
        readonly RfidConnectionProcessor _rfidProcessor;
        readonly ProximityConnectionProcessor _proximityProcessor;
        readonly WinBleAutomaticConnectionProcessor _winBleProcessor;
        readonly IWorkstationHelper _workstationHelper;

        UnlockSessionSwitchProc _unlockProcedure = null;

        internal UnlockSessionSwitchProc UnlockProcedure { get => _unlockProcedure; }

        public SessionUnlockMethodMonitor(ConnectionFlowProcessor connectionFlowProcessor,
                                          TapConnectionProcessor tapProcessor,
                                          RfidConnectionProcessor rfidProcessor,
                                          ProximityConnectionProcessor proximityProcessor,
                                          WinBleAutomaticConnectionProcessor winBleProcessor,
                                          IWorkstationHelper workstationHelper,
                                          ILog log): base(nameof(SessionUnlockMethodMonitor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor;
            _tapProcessor = tapProcessor;
            _rfidProcessor = rfidProcessor;
            _proximityProcessor = proximityProcessor;
            _winBleProcessor = winBleProcessor;
            _workstationHelper = workstationHelper;

            _connectionFlowProcessor.AttemptingUnlock += ConnectionFlowProcessor_AttemptingUnlock;
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }

        void ConnectionFlowProcessor_AttemptingUnlock(object sender, string flowId)
        {
            if (_workstationHelper.IsCurrentSessionLocked())
            {
                Task.Run(() =>
                {
                    _unlockProcedure = new UnlockSessionSwitchProc(flowId, _connectionFlowProcessor, _tapProcessor, _rfidProcessor, _proximityProcessor, _winBleProcessor);
                    Task.Run(_unlockProcedure.Run);
                    WriteLine("Started unlock procedure");
                });
            }
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            if (reason == SessionSwitchReason.SessionLogoff || reason == SessionSwitchReason.SessionLock)
                _unlockProcedure = null;
        }

        public async Task<SessionSwitchSubject> GetUnlockMethodAsync()
        {
            if (_unlockProcedure != null)
            {
                await _unlockProcedure.WaitFinish(); 

                if (_unlockProcedure.FlowFinished && _unlockProcedure.FlowUnlockResult.IsSuccessful)
                    return _unlockProcedure.UnlockMethod;
            }

            return SessionSwitchSubject.NonHideez;
        }
    }
}
