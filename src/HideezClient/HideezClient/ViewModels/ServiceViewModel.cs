using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using System;

namespace HideezClient.ViewModels
{
    class ServiceViewModel : ObservableObject, IDisposable
    {
        readonly IServiceProxy _serviceProxy;

        public bool IsServiceConnected
        {
            get 
            { 
                return _serviceProxy.IsConnected; 
            }
        }

        public ServiceViewModel(IServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;

            _serviceProxy.Connected += ServiceProxy_ConnectionStateChanged;
            _serviceProxy.Disconnected += ServiceProxy_ConnectionStateChanged;

        }

        void ServiceProxy_ConnectionStateChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(IsServiceConnected));
        }

        #region IDisposable Support
        bool disposed = false;

        protected virtual void Dispose(bool dispose)
        {
            if (!disposed)
            {
                if (dispose)
                {
                    _serviceProxy.Connected -= ServiceProxy_ConnectionStateChanged;
                    _serviceProxy.Disconnected -= ServiceProxy_ConnectionStateChanged;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
