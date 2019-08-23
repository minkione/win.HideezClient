using HideezMiddleware;
using ServiceLibrary.Implementation.SessionManagement;

namespace ServiceLibrary.Implementation.ScreenActivation
{
    class WcfScreenActivator : IScreenActivator
    {
        readonly ServiceClientSessionManager _sessionManager;

        public WcfScreenActivator(ServiceClientSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public void ActivateScreen()
        {
            foreach (var client in _sessionManager.Sessions)
                client.Callbacks.ActivateWorkstationScreenRequest();
        }
    }
}
