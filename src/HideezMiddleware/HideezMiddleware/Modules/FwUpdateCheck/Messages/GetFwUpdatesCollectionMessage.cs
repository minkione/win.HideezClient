using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.FwUpdateCheck.Messages
{
    class GetFwUpdatesCollectionMessage: PubSubMessageBase
    {
        public FwUpdateInfo[] FwUpdates { get; }

        public GetFwUpdatesCollectionMessage(FwUpdateInfo[] fwUpdates)
        {
            FwUpdates = fwUpdates;
        }
    }
}
