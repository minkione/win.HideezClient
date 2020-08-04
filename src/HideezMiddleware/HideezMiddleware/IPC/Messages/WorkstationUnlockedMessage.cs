using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class WorkstationUnlockedMessage : PubSubMessageBase
    {
        public bool IsNotHideezMethod { get; }

        public WorkstationUnlockedMessage(bool isNotHideezMethod)
        {
            IsNotHideezMethod = isNotHideezMethod;
        }
    }
}
