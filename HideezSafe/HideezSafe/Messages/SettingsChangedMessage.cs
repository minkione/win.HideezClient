using HideezSafe.Models.Settings;

namespace HideezSafe.Messages
{
    class SettingsChangedMessage<T> where T : BaseSettings, new()
    {
    }
}
