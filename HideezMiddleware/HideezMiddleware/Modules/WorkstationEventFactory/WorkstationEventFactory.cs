using Hideez.SDK.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
