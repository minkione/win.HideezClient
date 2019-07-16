using Hideez.SDK.Communication;
using HideezSafe.HideezServiceReference;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    interface IEventAggregator
    {
        Task PublishEventAsync(WorkstationEvent workstationEvent);
    }
}
