namespace HideezSafe.Utilities
{
    public interface IStartupHelper
    {
        bool AddToStartup();
        bool AddToStartup(string appName);
        bool AddToStartup(string appName, string path);
        bool IsInStartup();
        bool IsInStartup(string appName);
        bool RemoveFromStartup();
        bool RemoveFromStartup(string appName);
    }
}
