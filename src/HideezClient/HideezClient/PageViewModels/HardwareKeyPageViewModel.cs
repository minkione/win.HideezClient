using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using System;
using System.Threading.Tasks;

namespace HideezClient.PageViewModels
{
    class HardwareKeyPageViewModel : LocalizedObject
    {
        readonly IServiceProxy _serviceProxy;
        bool _showServiceAddressEdit = false;

        public bool ShowServiceAddressEdit
        {
            get { return _showServiceAddressEdit; }
            set { Set(ref _showServiceAddressEdit, value); }
        }

        public ServiceViewModel Service { get; }


        public HardwareKeyPageViewModel(IServiceProxy serviceProxy, ServiceViewModel serviceViewModel)
        {
            _serviceProxy = serviceProxy;
            Service = serviceViewModel;

            _serviceProxy.Connected += ServiceProxy_Connected;

            Task.Run(TryShowServerAddressEdit);
        }

        async void ServiceProxy_Connected(object sender, EventArgs e)
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
                    var address = await _serviceProxy.GetService().GetServerAddressAsync();
                    
                    if (string.IsNullOrWhiteSpace(address))
                        ShowServiceAddressEdit = true;
                }
            }
            catch (Exception) { }
        }
    }
}
