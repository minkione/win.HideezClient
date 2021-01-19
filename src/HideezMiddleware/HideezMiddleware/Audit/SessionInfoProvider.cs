using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware.Utils.WorkstationHelper;
using Microsoft.Win32;
using System;

namespace HideezMiddleware.Audit
{
    public class SessionInfoProvider : Logger, ISessionInfoProvider
    {
        readonly IWorkstationHelper _workstationHelper;
        public SessionInfo CurrentSession { get; private set; } = null;

        /// <summary>
        /// This property is used exclusivelly for lock event, because at that moment session id already changed
        /// </summary>
        public SessionInfo PreviousSession { get; private set; } = null;

        public SessionInfoProvider(IWorkstationHelper workstationHelper, ILog log)
            : base(nameof(SessionInfoProvider), log)
        {
            _workstationHelper = workstationHelper;

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            GenerateNewIdIfUnlocked();
        }

        void GenerateNewIdIfUnlocked()
        {
            var sid = _workstationHelper.GetSessionId();
            var state = _workstationHelper.GetSessionLockState(sid);
            var name = _workstationHelper.GetSessionName(sid);
            WriteLine($"Startup sid:{sid}, state:{state}, name:{name}");

            if (state == WorkstationInformationHelper.LockState.Unlocked && name != "SYSTEM")
            {
                GenerateNewSessionId();
            }
        }
        
        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            switch (reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                    WriteLine($"Session state changed to: locked (reason: {reason});");
                    ClearSessionId();
                    break;
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    WriteLine($"Session state changed to: unlocked (reason: {reason});");
                    GenerateNewSessionId();
                    break;
            }
        }

        void GenerateNewSessionId()
        {
            PreviousSession = CurrentSession;
            var virtualSessionId = Guid.NewGuid().ToString();
            var sessionName = _workstationHelper.GetSessionName(_workstationHelper.GetSessionId());
            CurrentSession = new SessionInfo(virtualSessionId, sessionName);
            WriteLine($"Generated new session id: (current: {CurrentSession?.SessionId}), (prev: {PreviousSession?.SessionId})");
        }

        void ClearSessionId()
        {
            PreviousSession = CurrentSession;
            CurrentSession = null;
            WriteLine($"Cleared session id: (current: {CurrentSession?.SessionId}), (prev: {PreviousSession?.SessionId})");
        }
    }
}