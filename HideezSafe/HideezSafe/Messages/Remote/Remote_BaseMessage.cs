namespace HideezSafe.Messages.Remote
{
    abstract class Remote_BaseMessage
    {
        public Remote_BaseMessage(string serialNo)
        {
            SerialNo = serialNo;
        }

        public string SerialNo { get; }
    }
}
