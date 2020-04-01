using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Interfaces;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using System;

namespace HideezClient.Models
{
    internal class SoftwareVaultModel : ObservableObject, IVaultModel, IDisposable
    {
        IServiceProxy _serviceProxy;
        IMessenger _messenger;
        ISoftwareVault _vault;

        public SoftwareVaultModel(
            IServiceProxy serviceProxy,
            IMessenger messenger,
            ISoftwareVault vault)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
            _vault = vault;

            RegisterDependencies();
        }

        #region Properties
        public IDynamicPasswordManager PasswordManager { get; private set; }

        #endregion

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
