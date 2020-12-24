using Hideez.SDK.Communication.Device;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    public interface IRemoteDeviceFactory
    {
        Task<Device> CreateRemoteDeviceAsync(string serialNo, byte channelNo, IMetaPubSub remoteDeviceMessenger);
    }
}