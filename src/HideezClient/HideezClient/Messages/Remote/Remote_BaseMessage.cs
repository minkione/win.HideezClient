namespace HideezClient.Messages.Remote
{
    abstract class Remote_BaseMessage
    {
        public Remote_BaseMessage(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
