using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using HideezClient.HideezServiceReference;
using HideezClient.Modules.ServiceProxy;

namespace HideezClient.Modules
{
    class EventPublisher : Logger, IEventPublisher
    {
        private readonly IServiceProxy serviceProxy;

        public EventPublisher(IServiceProxy serviceProxy, ILog log)
            : base(nameof(EventPublisher), log)
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
                WriteLine(ex);
            }
        }
    }
}
