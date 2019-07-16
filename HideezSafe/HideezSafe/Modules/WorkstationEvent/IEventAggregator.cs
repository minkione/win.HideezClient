using Hideez.SDK.Communication;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    interface IEventAggregator
    {
        Task PublishEventAsync(WorkstationEvent workstationEvent);
    }
}
