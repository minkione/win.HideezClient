using System;

namespace HideezMiddleware
{
    public static class PowerEventMonitor
    {
        public static event Action SystemQuerySuspend;
        public static event Action SystemLeftSuspendedMode;
        public static event Action SystemSuspending;

        public static void InvokeSystemQuerySuspendEvent()
        {
            SystemQuerySuspend?.Invoke();
        }

        public static void InvokeSystemLeftSuspendedModeEvent()
        {
            SystemLeftSuspendedMode?.Invoke();
        }

        public static void InvokeSystemSuspendingEvent()
        {
            SystemSuspending?.Invoke();
        }
    }
}
