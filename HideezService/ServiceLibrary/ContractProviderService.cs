using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public void DisconnectDevice(string deviceId)
        {
            throw new NotImplementedException();
        }

        public Task<string> EstablishRemoteDeviceConnection(string mac, byte channelNo)
        {
            throw new NotImplementedException();
        }

        public bool GetAdapterState(Adapter adapter)
        {
            throw new NotImplementedException();
        }

        public DeviceDTO[] GetDevices()
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

        public Task<byte[]> RemoteConnection_AuthCommandAsync(string serialNo, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> RemoteConnection_RemoteCommandAsync(string serialNo, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task RemoteConnection_ResetChannelAsync(string connectionId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveDeviceAsync(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
