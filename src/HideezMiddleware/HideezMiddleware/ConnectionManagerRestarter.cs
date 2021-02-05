using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    /// <summary>
    /// Automatically restarts connection manager if it enters unknown state
    /// </summary>
    public class ConnectionManagerRestarter : Logger
    {
        readonly List<IBleConnectionManager> _bleConnectionManagers = new List<IBleConnectionManager>();
        readonly object _managersLock = new object();
        CancellationTokenSource _tcs = null;
        readonly object _tcsLock = new object();

        public ConnectionManagerRestarter(ILog log)
            : base(nameof(ConnectionManagerRestarter), log)
        {
        }

        public void AddManager(IBleConnectionManager manager)
        {
            lock (_managersLock)
            {
                if (!_bleConnectionManagers.Contains(manager))
                    _bleConnectionManagers.Add(manager);
            }
        }

        public void RemoveManager(IBleConnectionManager manager)
        {
            lock(_managersLock)
            {
                _bleConnectionManagers.Remove(manager);
            }
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
                            manager.Restart();
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
