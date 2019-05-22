using HideezMiddleware;

namespace ServiceLibrary.Implementation
{
    class UiScreenActivator : IScreenActivator
    {
        readonly ServiceClientSessionManager _sessionManager;

        public UiScreenActivator(ServiceClientSessionManager sessionManager)
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
