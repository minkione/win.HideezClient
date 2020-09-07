using System;
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

        public Task DisconnectDevice(string deviceId)
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

        public void PublishEvent(WorkstationEventDTO workstationEvent)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> RemoteConnection_VerifyCommandAsync(string serialNo, byte[] data)
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

        public void SendPin(string deviceId, byte[] pin, byte[] oldPin)
        {
            throw new NotImplementedException();
        }

        public void CancelPin(string deviceId)
        {
            throw new NotImplementedException();
        }

        public byte[] GetAvailableChannels(string serialNo)
        {
            throw new NotImplementedException();
        }

        public void SetProximitySettings(string mac, int lockProximity, int unlockProximity)
        {
            throw new NotImplementedException();
        }

        public ProximitySettingsDTO GetCurrentProximitySettings(string mac)
        {
            throw new NotImplementedException();
        }

        public string GetServerAddress()
        {
            throw new NotImplementedException();
        }

        public Task<HideezMiddleware.ChangeServerAddressResult> ChangeServerAddress(string address)
        {
            throw new NotImplementedException();
        }

        public bool IsSoftwareVaultUnlockModuleEnabled()
        {
            throw new NotImplementedException();
        }

        public void SetSoftwareVaultUnlockModuleState(bool enabled)
        {
            throw new NotImplementedException();
        }

        public void SendActivationCode(string deviceId, byte[] activationCode)
        {
            throw new NotImplementedException();
        }

        public void CancelActivationCode(string deviceId)
        {
            throw new NotImplementedException();
        }
    }
}
