namespace HideezClient.Utilities
{
    internal interface IPublicSuffix
    {
        string GetTLD(string hostname);
    }
}
