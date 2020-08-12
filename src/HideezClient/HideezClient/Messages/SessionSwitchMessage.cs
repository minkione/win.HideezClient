using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;

namespace HideezClient.Messages
{
    public class SessionSwitchMessage: PubSubMessageBase
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
