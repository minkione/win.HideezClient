using Hideez.SDK.Communication;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezClient.HideezServiceReference;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface IEventAggregator
    {
        Task PublishEventAsync(WorkstationEvent workstationEvent);
    }
}
