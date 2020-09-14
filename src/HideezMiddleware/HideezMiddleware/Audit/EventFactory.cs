using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware.Workstation;
using System;

namespace HideezMiddleware.Audit
{
    class EventFactory
    {
        const string SYSTEM_SESSION_NAME = "SYSTEM";
        readonly SessionInfoProvider _sessionInfoProvider;
        readonly IWorkstationIdProvider _workstationIdProvider;

        public EventFactory(SessionInfoProvider sessionInfoProvider, IWorkstationIdProvider workstationIdProvider, ILog log)
        {
            _sessionInfoProvider = sessionInfoProvider;
            _workstationIdProvider = workstationIdProvider;
        }

        public WorkstationEvent GetWorkstationEvent()
        {
            return new WorkstationEvent
            {
                Version = WorkstationEvent.ClassVersion,
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                TimeZone = DateTimeOffset.Now,
                WorkstationId = _workstationIdProvider.GetWorkstationId(),
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
                TimeZone = DateTimeOffset.Now,
                WorkstationId = _workstationIdProvider.GetWorkstationId(),
                WorkstationSessionId = _sessionInfoProvider.PreviousSession?.SessionId,
                UserSession = _sessionInfoProvider.PreviousSession?.SessionName ?? SYSTEM_SESSION_NAME,
                Severity = WorkstationEventSeverity.Info,
            };
        }

    }
}
