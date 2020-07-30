using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace HideezClient.ViewModels
{
    class SoftwareUnlockSettingViewModel : LocalizedObject
    {
        readonly IMetaPubSub _metaMessenger;

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

        public SoftwareUnlockSettingViewModel(IMetaPubSub metaMessenger)
        {
            _metaMessenger = metaMessenger;

            _metaMessenger.TrySubscribeOnServer<ServiceSettingsChangedMessage>(OnServiceSettingsChanged);
            try
            {
                _metaMessenger.PublishOnServer(new RefreshServiceInfoMessage());
            }
            catch (Exception) { } // Handle error in case we are not connected to server
        }

        Task OnServiceSettingsChanged(ServiceSettingsChangedMessage arg)
        {
            _isChecked = arg.SoftwareVaultUnlockEnabled;
            return Task.CompletedTask;
        }

        async Task ApplyChangesInService(bool newValue)
        {
            try
            {
                await _metaMessenger.PublishOnServer(new SetSoftwareVaultUnlockModuleStateMessage(newValue));
            }
            catch (Exception) { }
        }
    }
}
