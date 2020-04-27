using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using System;
using System.Threading.Tasks;

namespace HideezClient.ViewModels
{
    class SoftwareUnlockSettingViewModel : LocalizedObject, IDisposable
    {
        readonly IServiceProxy _serviceProxy;

        bool _isChecked;

        public bool IsChecked
        {
            get { return _isChecked; }
            set 
            { 
                if (_isChecked != value)
                {
                    _isChecked = value;
                    NotifyPropertyChanged();
                    Task.Run(() => ApplyChangesInService(_isChecked)).ConfigureAwait(false);
                }
            }
        }

        public SoftwareUnlockSettingViewModel(IServiceProxy serviceProxy)
        {
            _serviceProxy = serviceProxy;

            _serviceProxy.Connected += ServiceProxy_Connected;

            Task.Run(InitializeViewModel).ConfigureAwait(false);
        }

        async void ServiceProxy_Connected(object sender, EventArgs e)
        {
            await InitializeViewModel();
        }

        async Task InitializeViewModel()
        {
            try
            {
                if (_serviceProxy.IsConnected)
                {
                    _isChecked = await _serviceProxy.GetService().IsSoftwareVaultUnlockModuleEnabledAsync();
                }
            }
            catch (Exception) { }
        }

        async Task ApplyChangesInService(bool newValue)
        {
            try
            {
                await _serviceProxy.GetService().SetSoftwareVaultUnlockModuleStateAsync(newValue);
            }
            catch (Exception) { }
        }

        #region IDisposable Support
        bool disposed = false;

        protected virtual void Dispose(bool dispose)
        {
            if (!disposed)
            {
                if (dispose)
                {
                    _serviceProxy.Connected -= ServiceProxy_Connected;
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
