using HideezSafe.Models;
using HideezSafe.ViewModels;

namespace HideezSafe.Modules
{
    public enum MenuItemType
    {
        ShowWindow,
        AddDevice,
        CheckForUpdates,
        ChangePassword,
        UserManual,
        TechnicalSupport,
        LiveChat,
        Legal,
        RFIDUsage,
        VideoTutorial,
        LogOff,
        Exit,
        Lenguage,
        Separator,
        LaunchOnStartup,
        GetLogsSubmenu,

        // For device
        AddCredential,
        DisconnectDevice,
        RemoveDevice,
    }

    public interface IMenuFactory
    {
        MenuItemViewModel GetMenuItem(MenuItemType type);
        MenuItemViewModel GetMenuItem(Device device, MenuItemType type);
    }
}
