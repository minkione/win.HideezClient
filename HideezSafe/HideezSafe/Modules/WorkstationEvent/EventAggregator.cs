using System.Threading.Tasks;
using Hideez.SDK.Communication;
using HideezSafe.HideezServiceReference;

namespace HideezSafe.Modules
{
    class EventAggregator : IEventAggregator
    {
        private readonly IHideezService hideezService;

        public EventAggregator(IHideezService hideezService)
        {
            this.hideezService = hideezService;
        }

        public async Task PublishEventAsync(WorkstationEvent workstationEvent)
        {
            WorkstationEventDTO we = new WorkstationEventDTO
            {
                ID = workstationEvent.ID,
                Date = workstationEvent.Date,
                Computer = workstationEvent.Computer,
                Event = (int)workstationEvent.Event,
                Status = (int)workstationEvent.Status,
                Note = workstationEvent.Note,
                DeviceSN = workstationEvent.DeviceSN,
                UserSession = workstationEvent.UserSession,
            };

            await hideezService.PublishEventAsync(we);
        }
    }
}
