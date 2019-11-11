using HideezClient.HideezServiceReference;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface IEventPublisher
    {
        Task PublishEventAsync(WorkstationEventDTO dto);
    }
}
