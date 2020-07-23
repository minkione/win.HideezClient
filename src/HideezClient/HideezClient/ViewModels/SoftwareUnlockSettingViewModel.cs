using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezMiddleware.IPC.IncommingMessages;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using System;
using System.Threading.Tasks;

namespace HideezClient.ViewModels
{
    class SoftwareUnlockSettingViewModel : LocalizedObject
    {
        readonly IServiceProxy _serviceProxy;
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

        public SoftwareUnlockSettingViewModel(IServiceProxy serviceProxy, IMetaPubSub metaMessenger)
        {
            _serviceProxy = serviceProxy;
            _metaMessenger = metaMessenger;

            _metaMessenger.Subscribe<ConnectedToServerEvent>(OnConnectedToServer, null);

            Task.Run(InitializeViewModel).ConfigureAwait(false);
        }

        async Task OnConnectedToServer(ConnectedToServerEvent args)
        {
            await InitializeViewModel();
        }

        async Task InitializeViewModel()
        {
            try
            {
                if (_serviceProxy.IsConnected)
                {
                    var reply = await _metaMessenger.ProcessOnServer<IsSoftwareVaultUnlockModuleEnabledReply>(new IsSoftwareVaultUnlockModuleEnabledMessage(), 500);
                    _isChecked = reply.IsEnabled;
                }
            }
            catch (Exception) { }
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
