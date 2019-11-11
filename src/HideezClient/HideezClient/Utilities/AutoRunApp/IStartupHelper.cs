using System;

namespace HideezClient.Utilities
{
    public enum AutoStartupState
    {
        On,
        Off,
    }

    public interface IStartupHelper
    {
        bool AddToStartup();
        bool AddToStartup(string appName);
        bool AddToStartup(string appName, string path);
        bool IsInStartup();
        bool IsInStartup(string appName);
        bool RemoveFromStartup();
        bool RemoveFromStartup(string appName);
        bool ReverseState();
        event Action<string, AutoStartupState> StateChanged;
    }
}
