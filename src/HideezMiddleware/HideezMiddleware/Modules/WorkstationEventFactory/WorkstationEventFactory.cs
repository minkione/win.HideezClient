using System;
using Hideez.SDK.Communication.WorkstationEvents;

namespace HideezMiddleware.Modules
{
    public class WorkstationEventFactory : IWorkstationEventFactory
    {
        public SdkWorkstationEvent GetBaseInitializedInstance()
        {
            var we = new SdkWorkstationEvent
            {
                Date = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                WorkstationId = Environment.MachineName,
            };

            return we;
        }
    }
}
