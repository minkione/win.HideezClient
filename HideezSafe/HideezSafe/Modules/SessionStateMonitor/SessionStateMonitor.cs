using Microsoft.Win32;
using System;

namespace HideezSafe.Modules.SessionStateMonitor
{
    class SessionStateMonitor : ISessionStateMonitor
    {
        SessionState currentState = SessionState.Unknown;
        DateTime lastStateSwitchTime = DateTime.UtcNow;

        public event EventHandler<SessionState> SessionStateChanged;

        public SessionState CurrentState
        {
            get
            {
                return currentState;
            }
            private set
            {
                if (currentState != value)
                {
                    currentState = value;
                    lastStateSwitchTime = DateTime.UtcNow;
                    OnStateChanged(currentState);
                }
            }
        }

        public DateTime LastSwitchTime
        {
            get
            {
                return lastStateSwitchTime;
            }
            private set
            {
                if (lastStateSwitchTime != value)
                {
                    lastStateSwitchTime = value;
                }
            }
        }

        public SessionStateMonitor()
        {
            CurrentState = GetSessionCurrentState();
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                CurrentState = SessionState.Locked;
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                CurrentState = SessionState.Unlocked;
            }
        }

        void OnStateChanged(SessionState newStatus)
        {
            SessionStateChanged?.Invoke(this, newStatus);
        }

        SessionState GetSessionCurrentState()
        {
            // TODO: Find a way to determine if current session is locked
            return SessionState.Unknown;
        }

        #region IDisposable
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
                }

                disposed = true;
            }
        }
        #endregion
    }
}
