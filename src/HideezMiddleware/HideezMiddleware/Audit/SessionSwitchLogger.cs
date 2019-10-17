using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.Tasks;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware.Audit
{
    // Todo: Code cleanup for SessionSwitchLogger
    public class SessionSwitchLogger : Logger, IDisposable
    {
        // There is a race condition between programmatic lock/unlock by our app and actual session switch
        const int LOCK_EVENT_TIMEOUT = 3_000;
        const int UNLOCK_EVENT_TIMEOUT = 10_000;

        class LockProcedure
        {
            public DateTime Time { get; set; }
            public string Mac { get; set; }
            public WorkstationLockingReason Reason { get; set; }
        }

        readonly EventSaver _eventSaver;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TapConnectionProcessor _tapProcessor;
        readonly RfidConnectionProcessor _rfidProcessor;
        readonly ProximityConnectionProcessor _proximityProcessor;
        readonly WorkstationLockProcessor _workstationLockProcessor;
        readonly BleDeviceManager _bleDeviceManager;

        LockProcedure _lockProcedure = null;
        object _lpLock = new object();

        UnlockSessionSwitchProc _unlockProcedure = null;
        object _upLock = new object();


        public SessionSwitchLogger(EventSaver eventSaver,
            ConnectionFlowProcessor connectionFlowProcessor,
            TapConnectionProcessor tapProcessor,
            RfidConnectionProcessor rfidProcessor,
            ProximityConnectionProcessor proximityProcessor,
            WorkstationLockProcessor workstationLockProcessor,
            BleDeviceManager bleDeviceManager,
            ILog log)
            : base(nameof(SessionSwitchLogger), log)
        {
            _eventSaver = eventSaver;
            _connectionFlowProcessor = connectionFlowProcessor;
            _tapProcessor = tapProcessor;
            _rfidProcessor = rfidProcessor;
            _proximityProcessor = proximityProcessor;
            _workstationLockProcessor = workstationLockProcessor;
            _bleDeviceManager = bleDeviceManager;

            _connectionFlowProcessor.Started += ConnectionFlowProcessor_Started;
            _workstationLockProcessor.WorkstationLocking += WorkstationLockProcessor_WorkstationLocking;
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }


        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _connectionFlowProcessor.Started -= ConnectionFlowProcessor_Started;
                    _workstationLockProcessor.WorkstationLocking -= WorkstationLockProcessor_WorkstationLocking;
                    SessionSwitchMonitor.SessionSwitch -= SessionSwitchMonitor_SessionSwitch;
                }

                disposed = true;
            }
        }

        ~SessionSwitchLogger()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        #endregion

        void WorkstationLockProcessor_WorkstationLocking(object sender, WorkstationLockingEventArgs e)
        {
            lock (_lpLock)
            {
                _lockProcedure = new LockProcedure()
                {
                    Time = DateTime.UtcNow,
                    Mac = e.Device.Mac,
                    Reason = e.Reason,
                };
            }
        }

        void ConnectionFlowProcessor_Started(object sender, string e)
        {
            lock (_upLock)
            {
                if (_unlockProcedure != null)
                    _unlockProcedure.Dispose();

                _unlockProcedure = new UnlockSessionSwitchProc(e, _connectionFlowProcessor, _tapProcessor, _rfidProcessor, _proximityProcessor);
            }
        }

        async void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            // If there is a better way of checking that enum belongs to a range values, use it
            switch (reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    break;
                default:
                    return; // Ignore all events except lock/unlock and logoff/logon
            }

            WorkstationEventType eventType = 0;

            bool isLock = false;
            bool isUnlock = false;

            switch (reason)
            {
                case SessionSwitchReason.SessionLock:
                    eventType = WorkstationEventType.ComputerLock;
                    isLock = true;
                    break;
                case SessionSwitchReason.SessionLogoff:
                    eventType = WorkstationEventType.ComputerLogoff;
                    isLock = true;
                    break;
                case SessionSwitchReason.SessionUnlock:
                    eventType = WorkstationEventType.ComputerUnlock;
                    isUnlock = true;
                    break;
                case SessionSwitchReason.SessionLogon:
                    eventType = WorkstationEventType.ComputerLogon;
                    isUnlock = true;
                    break;
            }

            try
            {
                if (isLock)
                    await OnWorkstationLock(sessionId, eventType);
                else if (isUnlock)
                    await OnWorkstationUnlock(sessionId, eventType);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        async Task OnWorkstationLock(int sessionId, WorkstationEventType eventType)
        {
            if (eventType != WorkstationEventType.ComputerLock && 
                eventType != WorkstationEventType.ComputerLogoff)
                return;

            var we = _eventSaver.GetPrevSessionWorkstationEvent();
            we.EventId = eventType;

            lock (_lpLock)
            {
                if (_lockProcedure != null && (DateTime.UtcNow - _lockProcedure.Time).TotalSeconds <= LOCK_EVENT_TIMEOUT)
                {
                    we.Note = _lockProcedure.Reason.ToString();
                    we.DeviceId = _bleDeviceManager.Find(_lockProcedure.Mac, 1)?.SerialNo; // Todo: Replace channel magic number with const
                }
                else
                    we.Note = WorkstationLockingReason.NonHideez.ToString();

                _lockProcedure = null;
            }

            await _eventSaver.AddNewAsync(we, true);
        }

        async Task OnWorkstationUnlock(int sessionId, WorkstationEventType eventType)
        {
            if (eventType != WorkstationEventType.ComputerUnlock &&
                eventType != WorkstationEventType.ComputerLogon)
                return;

            var time = DateTime.UtcNow;
            
            var procedure = _unlockProcedure;
            if (procedure != null)
                await procedure.Run(UNLOCK_EVENT_TIMEOUT);

            var we = _eventSaver.GetWorkstationEvent();
            we.EventId = eventType;
            we.Note = SessionSwitchSubject.NonHideez.ToString();
            we.Date = time;

            if (procedure != null && 
                procedure.FlowUnlockResult != null && 
                procedure.FlowUnlockResult.IsSuccessful)
            {
                we.Note = procedure.UnlockMethod.ToString();
                we.DeviceId = _bleDeviceManager.Find(procedure.FlowUnlockResult.DeviceMac, 1)?.SerialNo;
                we.AccountLogin = procedure.FlowUnlockResult.AccountLogin;
                we.AccountName = procedure.FlowUnlockResult.AccountName;
            }

            await _eventSaver.AddNewAsync(we, true);

        }
    }
}
