using Hideez.SDK.Communication.Remote;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    interface IRemoteDeviceFactory
    {
        Task<RemoteDevice> CreateRemoteDevice(string mac, byte channelNo);
    }
}