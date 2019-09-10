namespace HideezClient.Messages
{
    class ServiceErrorReceivedMessage
    {
        public string Id { get; set; }

        public string Message { get; set; }

        public ServiceErrorReceivedMessage(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }
}
