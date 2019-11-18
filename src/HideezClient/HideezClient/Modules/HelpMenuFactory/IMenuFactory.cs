using HideezClient.Models;
using HideezClient.ViewModels;

namespace HideezClient.Modules
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
        DisconnectDevice,
        RemoveDevice,
        AuthorizeDeviceAndLoadStorage,
        AboutDevice
    }

    public interface IMenuFactory
    {
        MenuItemViewModel GetMenuItem(MenuItemType type);
        MenuItemViewModel GetMenuItem(Device device, MenuItemType type);
    }
}
