using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class ChangeServerAddressMessageReply : PubSubMessageBase
    {
        public bool ChangedSuccessfully { get; set; }

        public ChangeServerAddressMessageReply(bool changedSuccessfully)
        {
            ChangedSuccessfully = changedSuccessfully;
        }
    }
}
