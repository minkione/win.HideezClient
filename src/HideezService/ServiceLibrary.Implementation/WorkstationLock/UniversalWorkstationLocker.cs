using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using Microsoft.Win32;
using ServiceLibrary.Implementation.ClientManagement;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.WorkstationLock
{
    class UniversalWorkstationLocker : Logger, IWorkstationLocker
    {
        class EnsureWorkstationLockProc : IDisposable
        {
            readonly int _lockTimeout;
            readonly WcfWorkstationLocker _wcfLocker;
            readonly WtsapiWorkstationLocker _wtsapiLocker;

            TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();

            public EnsureWorkstationLockProc(int lockTimeout, WcfWorkstationLocker wcfLocker, WtsapiWorkstationLocker wtsapiLocker)
            {
                _lockTimeout = lockTimeout;
                _wcfLocker = wcfLocker;
                _wtsapiLocker = wtsapiLocker;
            }

            public async Task Run()
            {
                /*
                if (WorkstationHelper.GetActiveSessionLockState() == WorkstationHelper.LockState.Unlocked)
                {
                    try
                    {
                        SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
                
                        _wcfLocker.LockWorkstation();
                
                        await Task.WhenAny(_tcs.Task, Task.Delay(_lockTimeout));
                    }
                    finally
                    {
                        SessionSwitchMonitor.SessionSwitch -= SessionSwitchMonitor_SessionSwitch;
                    }
                
                    if (!_tcs.Task.IsCompleted &&
                        WorkstationHelper.GetActiveSessionLockState() == WorkstationHelper.LockState.Unlocked)
                        _wtsapiLocker.LockWorkstation();
                }
                */
            }

            void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
            {
                if (reason == SessionSwitchReason.SessionLock ||
                    reason == SessionSwitchReason.SessionLogoff ||
                    reason == SessionSwitchReason.SessionLogon ||
                    reason == SessionSwitchReason.SessionUnlock)
                {
                    _tcs.SetResult(new object());
                }
            }

            #region IDisposable Support
            bool disposed = false; // To detect redundant calls
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            
            void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                    }

                    disposed = true;
                }
            }

            ~EnsureWorkstationLockProc()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }
            #endregion
        }

        readonly int _lockTimeout;
        readonly WcfWorkstationLocker _wcfLocker;
        readonly WtsapiWorkstationLocker _wtsapiLocker;

        public UniversalWorkstationLocker(int lockTimeout, ServiceClientSessionManager sessionManager, ILog log)
            : base(nameof(UniversalWorkstationLocker), log)
        {
            _lockTimeout = lockTimeout;
            _wcfLocker = new WcfWorkstationLocker(sessionManager, log);
            _wtsapiLocker = new WtsapiWorkstationLocker(log);
        }

        public void LockWorkstation()
        {
            EnsureWorkstationLockAsync();
        }

        void EnsureWorkstationLockAsync()
        {
            Task.Run(async () =>
            {
                using (var proc = new EnsureWorkstationLockProc(_lockTimeout, _wcfLocker, _wtsapiLocker))
                {
                    await proc.Run();
                }
            });
        }
    }
}
