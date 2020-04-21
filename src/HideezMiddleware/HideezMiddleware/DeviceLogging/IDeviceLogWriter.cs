using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;

namespace HideezMiddleware.DeviceLogging
{
    public interface IDeviceLogWriter
    {
        string SaveLog(string logsDirectoryPath, DailyDeviceLog dailyLog, IDevice device, bool includeDeviceMetadata);
    }
}