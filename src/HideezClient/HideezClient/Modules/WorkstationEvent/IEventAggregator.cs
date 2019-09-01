using Hideez.SDK.Communication.WorkstationEvents;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface IEventAggregator
    {
        Task PublishEventAsync(WorkstationEvent workstationEvent);
    }
}
