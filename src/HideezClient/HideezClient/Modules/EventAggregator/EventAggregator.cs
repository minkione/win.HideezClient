using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezClient.HideezServiceReference;
using HideezClient.Modules.ServiceProxy;
using NLog;

namespace HideezClient.Modules
{
    class EventAggregator : IEventAggregator
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProxy serviceProxy;

        public EventAggregator(IServiceProxy serviceProxy)
        {
            this.serviceProxy = serviceProxy;
        }

        public async Task PublishEventAsync(WorkstationEvent workstationEvent)
        {
            try
            {
                WorkstationEventDTO we = new WorkstationEventDTO
                {
                    Id = workstationEvent.Id,
                    Date = workstationEvent.Date,
                    WorkstationId = workstationEvent.WorkstationId,
                    EventId = (int)workstationEvent.EventId,
                    Severity = (int)workstationEvent.Severity,
                    Note = workstationEvent.Note,
                    DeviceId = workstationEvent.DeviceId,
                    UserSession = workstationEvent.UserSession,
                    AccountName = workstationEvent.AccountName,
                    AccountLogin = workstationEvent.AccountLogin,
                };

                await serviceProxy?.GetService()?.PublishEventAsync(we);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
