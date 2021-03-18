using HideezClient.Mvvm;
using HideezClient.ViewModels;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace HideezClient.PageViewModels
{
    class HardwareKeyPageViewModel : LocalizedObject
    {
        readonly IMetaPubSub _metaMessenger;
        bool _showServiceAddressEdit = false;

        public bool ShowServiceAddressEdit
        {
            get { return _showServiceAddressEdit; }
            set { Set(ref _showServiceAddressEdit, value); }
        }

        public ServiceViewModel Service { get; }

        public HardwareKeyPageViewModel(IMetaPubSub metaMessenger, ServiceViewModel serviceViewModel)
        {
            _metaMessenger = metaMessenger;
            Service = serviceViewModel;

            _metaMessenger.TrySubscribeOnServer<ServiceSettingsChangedMessage>(OnServiceSettingsChanged);
            try
            {
                _metaMessenger.PublishOnServer(new RefreshServiceInfoMessage());
            }
            catch (Exception) { } // Handle error in case we are not connected to server
        }

        Task OnServiceSettingsChanged(ServiceSettingsChangedMessage arg)
        {
            if (string.IsNullOrWhiteSpace(arg.ServerAddress))
                ShowServiceAddressEdit = true;

            return Task.CompletedTask;
        }
    }
}
