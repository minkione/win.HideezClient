using Hideez.SDK.Communication.WorkstationEvents;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    interface IEventAggregator
    {
        Task PublishEventAsync(WorkstationEvent workstationEvent);
    }
}
