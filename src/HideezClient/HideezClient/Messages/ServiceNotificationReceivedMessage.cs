namespace HideezClient.Messages
{
    class ServiceNotificationReceivedMessage
    {
        public string Message { get; set; }

        public ServiceNotificationReceivedMessage(string message)
        {
            Message = message;
        }
    }
}
