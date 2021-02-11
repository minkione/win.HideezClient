using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;

namespace HideezMiddleware.Modules.ServiceEvents.Messages
{
    internal sealed class SessionSwitchMonitor_SessionSwitchMessage : PubSubMessageBase
    {
        public int SessionId { get; }
        public SessionSwitchReason Reason { get; }

        public SessionSwitchMonitor_SessionSwitchMessage(int sessionId, SessionSwitchReason reason)
        {
            SessionId = sessionId;
            Reason = reason;
        }

    }
}
