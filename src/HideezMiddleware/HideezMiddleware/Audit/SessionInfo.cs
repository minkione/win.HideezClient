namespace HideezMiddleware.Audit
{
    class SessionInfo
    {
        public string SessionId { get; private set; } = string.Empty;

        public string SessionName { get; private set; } = string.Empty;

        public SessionInfo(string sessionId, string sessionName)
        {
            SessionId = sessionId;
            SessionName = sessionName;
        }
    }
}
