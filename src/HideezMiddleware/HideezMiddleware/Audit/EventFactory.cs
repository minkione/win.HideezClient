using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.WorkstationEvents;
using System;

namespace HideezMiddleware.Audit
{
    class EventFactory
    {
        SessionInfoProvider _sessionIdProvider;

        public EventFactory(ILog log)
        {
            _sessionIdProvider = new SessionInfoProvider(log);
        }

        public WorkstationEvent GetWorkstationEvent()
        {
            return new WorkstationEvent
            {
                Version = WorkstationEvent.ClassVersion,
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                WorkstationId = Environment.MachineName,
                WorkstationSessionId = _sessionIdProvider.CurrentSession?.SessionId,
                UserSession = _sessionIdProvider.CurrentSession?.SessionName,
                Severity = WorkstationEventSeverity.Info,
            };
        }

        public WorkstationEvent GetPreviousSessionEvent()
        {
            return new WorkstationEvent
            {
                Version = WorkstationEvent.ClassVersion,
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                WorkstationId = Environment.MachineName,
                WorkstationSessionId = _sessionIdProvider.PreviousSession?.SessionId,
                UserSession = _sessionIdProvider.PreviousSession?.SessionName,
                Severity = WorkstationEventSeverity.Info,
            };
        }

    }
}
