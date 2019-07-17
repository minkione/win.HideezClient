using System.Threading.Tasks;
using Hideez.SDK.Communication;
using HideezSafe.HideezServiceReference;
using HideezSafe.Modules.ServiceProxy;

namespace HideezSafe.Modules
{
    class EventAggregator : IEventAggregator
    {
        private readonly IServiceProxy serviceProxy;

        public EventAggregator(IServiceProxy serviceProxy)
        {
            this.serviceProxy = serviceProxy;
        }

        public async Task PublishEventAsync(WorkstationEvent workstationEvent)
        {
            WorkstationEventDTO we = new WorkstationEventDTO
            {
                ID = workstationEvent.Id,
                Date = workstationEvent.Date,
                Computer = workstationEvent.Computer,
                Event = (int)workstationEvent.EventId,
                Status = (int)workstationEvent.Severity,
                Note = workstationEvent.Note,
                DeviceSN = workstationEvent.DeviceId,
                UserSession = workstationEvent.UserSession,
                AccountName = workstationEvent.AccountName,
            };

            await serviceProxy?.GetService()?.PublishEventAsync(we);
        }
    }
}
