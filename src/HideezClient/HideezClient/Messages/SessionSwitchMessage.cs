using Microsoft.Win32;

namespace HideezClient.Messages
{
    class SessionSwitchMessage
    {
        public SessionSwitchMessage(int sessionId, SessionSwitchReason reason)
        {
            SessionId = sessionId;
            Reason = reason;
        }

        public int SessionId { get; private set; }

        public SessionSwitchReason Reason { get; private set; }
    }
}
