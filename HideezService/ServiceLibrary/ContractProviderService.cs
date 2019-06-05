using System;
using System.Collections.Generic;

namespace ServiceLibrary
{
    /// <summary>
    /// This service is used to provide automatic contract generation for other projects
    /// All interface members of IHideezService can be left as not implemented
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

        public void DisableMonitoringDeviceProperties(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void DisableMonitoringProximity(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void DisconnectDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void EnableMonitoringDeviceProperties(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void EnableMonitoringProximity(string deviceId)
        {
            throw new NotImplementedException();
        }

        public bool GetAdapterState(Adapter addapter)
        {
            throw new NotImplementedException();
        }

        public DeviceDTO[] GetPairedDevices()
        {
            throw new NotImplementedException();
        }

        public void OnSessionChange(bool sessionLocked)
        {
            throw new NotImplementedException();
        }

        public int Ping()
        {
            throw new NotImplementedException();
        }

        public void RemoveDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void SaveCredential(string deviceId, string login, string password)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
