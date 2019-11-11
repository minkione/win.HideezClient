using Hideez.SDK.Communication.Remote;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    public interface IRemoteDeviceFactory
    {
        Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo);
    }
}