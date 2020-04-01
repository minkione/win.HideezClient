using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using System;
using System.Threading.Tasks;

namespace HideezClient.Models
{
    internal class SoftwareVaultModel : ObservableObject, IVaultModel
    {
        IMessenger _messenger;
        ISoftwareVault _vault;
        ILog _log;

        public SoftwareVaultModel(
            IMessenger messenger,
            ISoftwareVault vault,
            ILog log)
        {
            _messenger = messenger;
            _vault = vault;
            _log = log;

            RegisterDependencies();

            PasswordManager = new SoftwareVaultPasswordManager(_vault, _log);
        }

        #region Properties
        public IDynamicPasswordManager PasswordManager { get; private set; }

        public string Id => throw new NotImplementedException();

        public string SerialNo => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public bool IsInitialized => throw new NotImplementedException();

        public bool IsAuthorized => throw new NotImplementedException();

        public bool IsStorageLoaded => throw new NotImplementedException();
        #endregion

        public Task ShutdownRemoteDeviceAsync(HideezErrorCode deviceRemoved)
        {
            throw new NotImplementedException();
        }

        public Task InitRemoteAndLoadStorageAsync(bool authorize = true)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Support
        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _messenger.Unregister(this);
                }

                disposed = true;
            }
        }

        ~SoftwareVaultModel()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
