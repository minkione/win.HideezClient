using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using HideezClient.HideezServiceReference;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;

namespace HideezClient.Modules
{
    class EventPublisher : IEventPublisher
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(EventPublisher));
        readonly IServiceProxy serviceProxy;

        public EventPublisher(IServiceProxy serviceProxy)
        {
            this.serviceProxy = serviceProxy;
        }

        public async Task PublishEventAsync(WorkstationEventDTO dto)
        {
            try
            { 
                await serviceProxy?.GetService()?.PublishEventAsync(dto);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }
    }
}
