using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Remote;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    public interface IRemoteDeviceFactory
    {
        Task<Device> CreateRemoteDeviceAsync(string serialNo, byte channelNo, IMetaPubSub remoteDeviceMessenger);
    }
}