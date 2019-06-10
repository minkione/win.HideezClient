﻿using System;
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

        public Task SaveCredentialAsync(string deviceId, string login, string password)
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

        public Task<string> EstablishRemoteDeviceConnection(string mac, byte channelNo)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> RemoteConnection_AuthCommandAsync(string connectionId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task RemoteConnection_ResetChannelAsync(string connectionId)
        {
            throw new NotImplementedException();
        }
    }
}
