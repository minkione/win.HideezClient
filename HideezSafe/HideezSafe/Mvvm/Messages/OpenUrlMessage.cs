namespace HideezSafe.Mvvm.Messages
{
    class OpenUrlMessage
    {
        public OpenUrlMessage(string url)
        {
            this.Url = url;
        }

        public string Url { get; }
    }
}
