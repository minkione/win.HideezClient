using Hideez.SDK.Communication;

namespace HideezMiddleware.Modules
{
    public interface IWorkstationEventFactory
    {
        WorkstationEvent GetBaseInitializedInstance();
    }
}