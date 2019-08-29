using Hideez.SDK.Communication.Log;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    class BaseConnectionProcessor : Logger
    {
        const int TIMEOUT_MS = 60 * 1000 * 2; // 2 minutes
        readonly ConnectionFlowProcessor _connectionFlowProcessor;

        public BaseConnectionProcessor(ConnectionFlowProcessor connectionFlowProcessor, string logSource, ILog log)
            : base(logSource, log)
        {
            _connectionFlowProcessor = connectionFlowProcessor;
        }

        public virtual async Task ConnectDeviceByMac(string mac)
        {
            var connectionTask = _connectionFlowProcessor.ConnectAndUnlock(mac);
            var timeoutNotificationTask = Task.Delay(TIMEOUT_MS);

            connectionTask.Start();
            timeoutNotificationTask.Start();

            Task.WaitAny(connectionTask, timeoutNotificationTask);

            if (timeoutNotificationTask.IsCompleted && !connectionTask.IsCompleted)
                WriteLine($"Device connection not finished after {TIMEOUT_MS} ms", LogErrorSeverity.Error);
        }

    }
}
