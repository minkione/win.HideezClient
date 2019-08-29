using Hideez.SDK.Communication.WorkstationEvents;

namespace HideezMiddleware.Modules
{
    public interface IWorkstationEventFactory
    {
        SdkWorkstationEvent GetBaseInitializedInstance();
    }
}