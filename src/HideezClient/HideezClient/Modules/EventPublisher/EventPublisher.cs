using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using HideezMiddleware.IPC.DTO;
using HideezMiddleware.IPC.IncommingMessages;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Modules
{
    class EventPublisher : IEventPublisher
    {
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(EventPublisher));
        readonly IServiceProxy serviceProxy;
        readonly IMetaPubSub _metaMessenger;

        public EventPublisher(IServiceProxy serviceProxy, IMetaPubSub metaMessenger)
        {
            this.serviceProxy = serviceProxy;
            _metaMessenger = metaMessenger;
        }

        public async Task PublishEventAsync(WorkstationEventDTO dto)
        {
            try
            {
                await _metaMessenger.PublishOnServer(new PublishEventMessage(dto));
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }
    }
}
