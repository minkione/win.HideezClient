using System;

namespace ServiceLibrary
{
    /// <summary>
    /// This service is used to provide automatic contract generation for other projects
    /// </summary>
    class ContractProviderService : IHideezService
    {
        public bool AttachClient(ServiceClientParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void DetachClient()
        {
            throw new NotImplementedException();
        }

        public byte[] Ping(byte[] ping)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
