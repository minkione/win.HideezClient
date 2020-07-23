using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using HideezMiddleware.IPC.IncommingMessages;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using System;
using System.Threading.Tasks;

namespace HideezClient.PageViewModels
{
    class HardwareKeyPageViewModel : LocalizedObject
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMetaPubSub _metaMessenger;
        bool _showServiceAddressEdit = false;

        public bool ShowServiceAddressEdit
        {
            get { return _showServiceAddressEdit; }
            set { Set(ref _showServiceAddressEdit, value); }
        }

        public ServiceViewModel Service { get; }


        public HardwareKeyPageViewModel(IServiceProxy serviceProxy, IMetaPubSub metaMessenger, ServiceViewModel serviceViewModel)
        {
            _serviceProxy = serviceProxy;
            _metaMessenger = metaMessenger;
            Service = serviceViewModel;

            _metaMessenger.Subscribe<ConnectedToServerEvent>(OnConnectedToServer, null);

            Task.Run(TryShowServerAddressEdit);
        }

        async Task OnConnectedToServer(ConnectedToServerEvent args)
        {
            await TryShowServerAddressEdit();
        }

        /// <summary>
        /// Check saved server address. If server address is null or empty, display server address edit control.
        /// </summary>
        async Task TryShowServerAddressEdit()
        {
            try
            {
                if (_serviceProxy.IsConnected)
                {
                    var reply = await _metaMessenger.ProcessOnServer<GetServerAddressMessageReply>(new GetServerAddressMessage(), 0);

                    if (string.IsNullOrWhiteSpace(reply.ServerAddress))
                        ShowServiceAddressEdit = true;
                }
            }
            catch (Exception) { }
        }
    }
}
