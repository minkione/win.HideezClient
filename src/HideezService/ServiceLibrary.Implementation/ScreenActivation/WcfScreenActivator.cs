using HideezMiddleware.ScreenActivation;
using ServiceLibrary.Implementation.ClientManagement;
using System;

namespace ServiceLibrary.Implementation.ScreenActivation
{
    class WcfScreenActivator : ScreenActivator
    {
        readonly ServiceClientSessionManager _sessionManager;

        public WcfScreenActivator(ServiceClientSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public override void ActivateScreen()
        {
            foreach (var client in _sessionManager.Sessions)
            {
                try
                {
                    client.Callbacks.ActivateWorkstationScreenRequest();
                }
                catch (Exception ex) { }
            }
        }
    }
}
