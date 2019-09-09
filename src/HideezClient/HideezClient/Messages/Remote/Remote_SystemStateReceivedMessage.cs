namespace HideezClient.Messages.Remote
{
    class Remote_SystemStateReceivedMessage : Remote_BaseMessage
    {
        public byte[] SystemStateData { get; }

        public Remote_SystemStateReceivedMessage(string id, byte[] systemStateData) 
            : base(id)
        {
            SystemStateData = systemStateData;
        }
    }
}
