using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow.ConnectionFlow;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection
{
    /// <summary>
    /// Connection flow is initiated by an external action outside of application scope
    /// which is detected by appearance of new controller
    /// </summary>
    public sealed class ExternalConnectionProcessor : BaseConnectionProcessor, IDisposable
    {
        readonly IConnectionManager _connectionManager;
        readonly object _lock = new object();

        int _isConnecting = 0;
        bool isRunning = false;

        public ExternalConnectionProcessor(
            ConnectionFlowProcessorBase connectionFlowProcessor,
            IConnectionManager connectionManager,
            ILog log)
            : base(connectionFlowProcessor, nameof(ExternalConnectionProcessor), log)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _connectionManager.ControllerAdded -= ConnectionManager_ControllerAdded;
            }

            disposed = true;
        }

        ~ExternalConnectionProcessor()
        {
            Dispose(false);
        }
        #endregion

        public override void Start()
        {
            lock (_lock)
            {
                if (!isRunning)
                {
                    _connectionManager.ControllerAdded += ConnectionManager_ControllerAdded;
                    isRunning = true;
                    WriteLine("Started");
                }
            }
        }

        public override void Stop()
        {
            lock (_lock)
            {
                isRunning = false;
                _connectionManager.ControllerAdded -= ConnectionManager_ControllerAdded;
                WriteLine("Stopped");
            }
        }

        async void ConnectionManager_ControllerAdded(object sender, ControllerAddedEventArgs args)
        {
            await UnlockByExternalConnect(args);
        }

        async Task UnlockByExternalConnect(ControllerAddedEventArgs args)
        {
            if (!isRunning)
                return;

            if (args.Controller == null)
                return;

            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    await ConnectAndUnlockByConnectionId(args.Controller.Connection.ConnectionId);
                }
                catch (Exception)
                {
                    // Silent handling. Log is already printed inside of _connectionFlowProcessor.ConnectAndUnlock()
                }
                finally
                {
                    // this delay allows a user to move away the device from the dongle
                    // and prevents the repeated call of this method
                    await Task.Delay(SdkConfig.DelayAfterMainWorkflow);

                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
        }

    }
}
