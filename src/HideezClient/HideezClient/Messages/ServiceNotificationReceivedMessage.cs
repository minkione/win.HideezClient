namespace HideezClient.Messages
{
    class ServiceNotificationReceivedMessage
    {
        public string Id { get; set; }

        public string Message { get; set; }

        public ServiceNotificationReceivedMessage(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }
}
