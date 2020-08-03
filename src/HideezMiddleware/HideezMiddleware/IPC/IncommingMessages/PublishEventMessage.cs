using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class PublishEventMessage : PubSubMessageBase
    {
        public WorkstationEventDTO WorkstationEvent { get; set; }

        public PublishEventMessage(WorkstationEventDTO workstationEvent)
        {
            WorkstationEvent = workstationEvent;
        }
    }
}
