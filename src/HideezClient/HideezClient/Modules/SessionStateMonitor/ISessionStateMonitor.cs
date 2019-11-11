using System;

namespace HideezClient.Modules.SessionStateMonitor
{
    public enum SessionState
    {
        Unknown = 0,
        Locked = 1,
        Unlocked = 2,
    }

    interface ISessionStateMonitor
    {
        event EventHandler<SessionState> SessionStateChanged;

        SessionState CurrentState { get; }

        DateTime LastSwitchTime { get; }
    }

}
