using HideezMiddleware.Settings;
using HideezClient.Models.Settings;

namespace HideezClient.Messages
{
    class SettingsChangedMessage<T> where T : BaseSettings, new()
    {
    }
}
