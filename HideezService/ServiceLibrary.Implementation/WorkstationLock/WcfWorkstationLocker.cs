using HideezMiddleware;
using NLog;
using ServiceLibrary.Implementation.SessionManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.WorkstationLock
{
    class WcfWorkstationLocker : IWorkstationLocker
    {
        Logger _log = LogManager.GetCurrentClassLogger();
        readonly ServiceClientSessionManager _sessionManager;

        public WcfWorkstationLocker(ServiceClientSessionManager sessionManager)
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
                try
                {
                    foreach (var client in _sessionManager.Sessions.ToList())
                        client.Callbacks.LockWorkstationRequest();
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                }
            });
        }
    }
}
