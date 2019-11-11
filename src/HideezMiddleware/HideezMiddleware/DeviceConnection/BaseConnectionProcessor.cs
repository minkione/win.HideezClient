//using Hideez.SDK.Communication.Log;
//using Hideez.SDK.Communication.Utils;
//using System.Threading.Tasks;

//namespace HideezMiddleware.DeviceConnection
//{
//    public class BaseConnectionProcessor : Logger
//    {
//        const int TIMEOUT_MS = 60 * 1000 * 2; // 2 minutes
//        readonly ConnectionFlowProcessor _connectionFlowProcessor;

//        public BaseConnectionProcessor(ConnectionFlowProcessor connectionFlowProcessor, string logSource, ILog log)
//            : base(logSource, log)
//        {
//            _connectionFlowProcessor = connectionFlowProcessor;
//        }

//        public virtual async Task ConnectDeviceByMac(string mac, int timeout)
//        {
//            await _connectionFlowProcessor.ConnectAndUnlock(mac).TimeoutAfter(10000);
//            //var connectionTask = _connectionFlowProcessor.ConnectAndUnlock(mac);
//            //var timeoutNotificationTask = Task.Delay(TIMEOUT_MS);

//            //var firstFinishedTask = await Task.WhenAny(connectionTask, timeoutNotificationTask);

//            //if (timeoutNotificationTask.IsCompleted && !connectionTask.IsCompleted)
//            //    WriteLine($"Device connection not finished after {TIMEOUT_MS} ms", LogErrorSeverity.Error);
//        }

//    }
//}
