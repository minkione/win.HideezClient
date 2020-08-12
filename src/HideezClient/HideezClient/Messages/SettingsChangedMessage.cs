using HideezMiddleware.Settings;
using HideezClient.Models.Settings;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class SettingsChangedMessage<T>: PubSubMessageBase where T : BaseSettings, new()
    {
    }
}
