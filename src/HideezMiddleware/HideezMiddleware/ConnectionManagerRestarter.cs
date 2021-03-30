using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    /// <summary>
    /// Automatically restarts connection manager if it enters unknown state
    /// </summary>
    public class ConnectionManagerRestarter : Logger
    {
        readonly IBleConnectionManager[] _bleConnectionManagers;
        CancellationTokenSource _tcs = null;
        object _tcsLock = new object();

        public ConnectionManagerRestarter(ILog log, params IBleConnectionManager[] bleConnectionManagers)
            : base(nameof(ConnectionManagerRestarter), log)
        {
            _bleConnectionManagers = bleConnectionManagers;
        }

        public int ManagerStateCheckIntervalMs { get; set; } = 2000;

        public void Start()
        {
            lock (_tcsLock)
            {
                if (_tcs == null)
                {
                    _tcs = new CancellationTokenSource();
                    Task.Run(InfiniteRestartLoop);
                }
            }
        }

        public void Stop()
        {
            lock (_tcsLock)
            {
                if (_tcs != null)
                {
                    WriteLine("Cancellation requested");
                    _tcs.Cancel();
                    _tcs = null;
                }
            }
        }
        
        async Task InfiniteRestartLoop()
        {
            CancellationToken token;
            lock (_tcsLock)
            {
                if (_tcs == null)
                    return;

                token = _tcs.Token;
            }
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(ManagerStateCheckIntervalMs, token);

                if (token.IsCancellationRequested)
                    break;

                foreach (var manager in _bleConnectionManagers)
                {
                    if (manager.State == BluetoothAdapterState.Unknown)
                    {
                        try
                        {
                            await manager.Restart();
                        }
                        catch (Exception ex)
                        {
                            WriteLine(ex);
                            continue;
                        }
                    }
                }
            }
            
            WriteLine("Restart loop ended");
        }
    }
}
