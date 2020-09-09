using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class ChangeServerAddressMessageReply : PubSubMessageBase
    {
        public ChangeServerAddressResult Result { get; set; }

        public ChangeServerAddressMessageReply(ChangeServerAddressResult result)
        {
            Result = result;
        }
    }
}
