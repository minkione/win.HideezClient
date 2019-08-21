namespace HideezSafe.Messages
{
    class ServiceErrorReceivedMessage
    {
        public string Message { get; set; }

        public ServiceErrorReceivedMessage(string message)
        {
            Message = message;
        }
    }
}
