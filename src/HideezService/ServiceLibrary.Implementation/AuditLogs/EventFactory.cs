using Hideez.SDK.Communication;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware;
using System;

namespace ServiceLibrary.Implementation.AuditLogs
{
    class EventFactory
    {
        public static WorkstationEvent GetWorkstationEvent()
        {
            var sessionInfo = WorkstationHelper.GetSessionInfo();

            return new WorkstationEvent
            {
                Version = WorkstationEvent.ClassVersion,
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                WorkstationId = Environment.MachineName,
                WorkstationSessionId = Convert.ToString(sessionInfo.SessionId),
                UserSession = sessionInfo.SessionName,
                Severity = WorkstationEventSeverity.Info,
            };
        }


    }
}
