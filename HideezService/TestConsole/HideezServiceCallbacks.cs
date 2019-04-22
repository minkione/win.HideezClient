using TestConsole.HideezServiceReference;

namespace TestConsole
{
    class HideezServiceCallbacks : IHideezServiceCallback
    {
        public void ConnectionDongleChangedRequest(bool isConnected)
        {
        }

        public void ConnectionHESChangedRequest(bool isConnected)
        {
        }

        public void ConnectionRFIDChangedRequest(bool isConnected)
        {
        }

        public void LockWorkstationRequest()
        {
        }
    }
}
