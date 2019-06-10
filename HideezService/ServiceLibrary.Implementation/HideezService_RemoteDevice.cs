using Hideez.SDK.Communication.WCF;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        public async Task<string> EstablishRemoteDeviceConnection(string mac, byte channelNo)
        {
            var connection = await _wcfDeviceManager.EstablishRemoteDeviceConnection(mac, channelNo);

            return connection.Id;
        }

        public async Task<byte[]> RemoteConnection_AuthCommandAsync(string connectionId, byte[] data)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                var response = await wcfDevice.OnAuthCommandAsync(data);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
                return null; // this line is unreachable
            }
        }

        public async Task<byte[]> RemoteConnection_RemoteCommandAsync(string connectionId, byte[] data)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                var response = await wcfDevice.OnRemoteCommandAsync(data);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
                return null; // this line is unreachable
            }
        }

        public async Task RemoteConnection_ResetChannelAsync(string connectionId)
        {
            try
            {
                var wcfDevice = (IWcfDevice)_deviceManager.Find(connectionId);

                await wcfDevice.OnResetChannelAsync();
            }
            catch (Exception ex)
            {
                LogException(ex);
                ThrowException(ex);
            }
        }


    }
}
