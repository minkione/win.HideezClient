using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class GetAvailableChannelsMessage : PubSubMessageBase
    {
        public string SerialNo { get; set; }

        public GetAvailableChannelsMessage(string serialNo)
        {
            SerialNo = serialNo;
        }
    }
}
