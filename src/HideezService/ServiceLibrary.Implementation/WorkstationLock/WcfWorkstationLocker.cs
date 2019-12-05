using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using ServiceLibrary.Implementation.ClientManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.WorkstationLock
{
    class WcfWorkstationLocker : Logger, IWorkstationLocker
    {
        readonly ServiceClientSessionManager _sessionManager;

        public WcfWorkstationLocker(ServiceClientSessionManager sessionManager, ILog log)
            : base(nameof(WcfWorkstationLocker), log)
        {
            _sessionManager = sessionManager;
        }

        public void LockWorkstation()
        {
            LockWorkstationAsync();
        }

        void LockWorkstationAsync()
        {
            Task.Run(() =>
            {
                var lockState = WorkstationHelper.GetCurrentSessionLockState();
                if (lockState == WorkstationHelper.LockState.Unlocked)
                {
                    foreach (var client in _sessionManager.Sessions)
                    {
                        try
                        {
                            client.Callbacks.LockWorkstationRequest();
                        }
                        catch (Exception ex)
                        {
                            WriteLine(ex);
                        }
                    }
                }
            });
        }
    }
}
