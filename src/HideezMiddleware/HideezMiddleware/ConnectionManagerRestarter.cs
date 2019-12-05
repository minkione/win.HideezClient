using Hideez.SDK.Communication.BLE;
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
        IBleConnectionManager _bleConnectionManager;
        CancellationTokenSource _tcs = null;
        object _tcsLock = new object();

        public ConnectionManagerRestarter(IBleConnectionManager bleConnectionManager, ILog log)
            : base(nameof(ConnectionManagerRestarter), log)
        {
            _bleConnectionManager = bleConnectionManager ?? throw new ArgumentNullException(nameof(bleConnectionManager));
        }

        public int CheckIntervalMs { get; set; } = 2000;

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
                await Task.Delay(2000, token);

                if (token.IsCancellationRequested)
                    break;

                if (_bleConnectionManager.State == BluetoothAdapterState.Unknown)
                {
                    try
                    {
                        WriteLine("Restarting connection manager due to Unknown state");
                        _bleConnectionManager.Restart();
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex);
                        return;
                    }
                }
            }
            
            WriteLine("Restart loop ended");
        }
    }
}
