using Hideez.SDK.Communication.Proximity;

namespace ServiceLibrary.Implementation
{
    class UiWorkstationLocker : IWorkstationLocker
    {
        readonly ServiceClientSessionManager _sessionManager;

        public UiWorkstationLocker(ServiceClientSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public void LockWorkstation()
        {
            foreach (var client in _sessionManager.Sessions)
                client.Callbacks.LockWorkstationRequest();
        }
    }
}
