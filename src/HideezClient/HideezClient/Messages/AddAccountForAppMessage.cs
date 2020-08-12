using Hideez.ARM;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class AddAccountForAppMessage: PubSubMessageBase
    {
        public string DeviceId { get; }

        public AppInfo AppInfo { get; }

        public AddAccountForAppMessage(string deviceId, AppInfo appInfo)
        {
            DeviceId = deviceId;
            AppInfo = appInfo;
        }
    }
}
