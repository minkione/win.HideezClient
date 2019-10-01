using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using Hideez.SDK.Communication.WorkstationEvents;
using System;

namespace HideezMiddleware.Audit
{
    class EventFactory
    {
        const string SYSTEM_SESSION_NAME = "SYSTEM";
        SessionInfoProvider _sessionInfoProvider;

        public EventFactory(SessionInfoProvider sessionInfoProvider, ILog log)
        {
            _sessionInfoProvider = sessionInfoProvider;
        }

        public WorkstationEvent GetWorkstationEvent()
        {
            return new WorkstationEvent
            {
                Version = WorkstationEvent.ClassVersion,
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                WorkstationId = Environment.MachineName,
                WorkstationSessionId = _sessionInfoProvider.CurrentSession?.SessionId,
                UserSession = _sessionInfoProvider.CurrentSession?.SessionName ?? SYSTEM_SESSION_NAME,
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
                WorkstationSessionId = _sessionInfoProvider.PreviousSession?.SessionId,
                UserSession = _sessionInfoProvider.PreviousSession?.SessionName ?? SYSTEM_SESSION_NAME,
                Severity = WorkstationEventSeverity.Info,
            };
        }

    }
}
