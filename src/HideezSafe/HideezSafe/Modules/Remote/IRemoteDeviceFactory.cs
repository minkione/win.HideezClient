using Hideez.SDK.Communication.Remote;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    public interface IRemoteDeviceFactory
    {
        Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo);
    }
}