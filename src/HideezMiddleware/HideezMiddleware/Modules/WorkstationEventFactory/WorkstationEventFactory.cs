using System;
using Hideez.SDK.Communication.WorkstationEvents;

//todo - same functionality as in: public static WorkstationEvent GetBaseInitializedInstance()
// leave one of the approaches
namespace HideezMiddleware.Modules
{
    public class WorkstationEventFactory : IWorkstationEventFactory
    {
        public WorkstationEvent GetBaseInitializedInstance()
        {
            var we = new WorkstationEvent
            {
                Date = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                WorkstationId = Environment.MachineName,
            };

            return we;
        }
    }
}
