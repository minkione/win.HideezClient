using Hideez.SDK.Communication.Remote;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    public interface IRemoteDeviceFactory
    {
        Task<RemoteDevice> CreateRemoteDeviceAsync(string serialNo, byte channelNo, IMetaPubSub remoteDeviceMessenger);
    }
}