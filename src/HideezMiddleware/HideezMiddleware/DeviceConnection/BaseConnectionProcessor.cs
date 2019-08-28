using Hideez.SDK.Communication.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    class BaseConnectionProcessor : Logger
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;

        public BaseConnectionProcessor(ConnectionFlowProcessor connectionFlowProcessor, string logSource, ILog log)
            : base(logSource, log)
        {
            _connectionFlowProcessor = connectionFlowProcessor;
        }

        public virtual async Task ConnectDeviceByMac(string mac)
        {
            await Task.CompletedTask;
            // Todo: Call main workflow from _connectionFlowProcessor
        }

    }
}
