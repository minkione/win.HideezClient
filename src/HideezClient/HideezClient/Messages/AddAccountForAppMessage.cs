using Hideez.ARM;

namespace HideezClient.Messages
{
    class AddAccountForAppMessage
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
