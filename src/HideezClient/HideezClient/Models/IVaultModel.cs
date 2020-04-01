using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HideezClient.Models
{
    public interface IVaultModel : INotifyPropertyChanged, IDisposable
    {
        string Id { get; }

        bool IsConnected { get; }
        bool IsInitialized { get; }
        bool IsAuthorized { get; }
        bool IsStorageLoaded { get; }

        IDynamicPasswordManager PasswordManager { get; }
        string SerialNo { get; }

        Task ShutdownRemoteDeviceAsync(HideezErrorCode deviceRemoved);

        Task InitRemoteAndLoadStorageAsync(bool authorize = true);
    }
}
