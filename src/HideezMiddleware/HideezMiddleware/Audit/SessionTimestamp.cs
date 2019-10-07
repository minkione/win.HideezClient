using System;

namespace HideezMiddleware.Audit
{
    [Serializable]
    class SessionTimestamp
    {
        public DateTime Time { get; set; }

        public string SessionId { get; set; }

        public string SessionName { get; set; }
    }
}
