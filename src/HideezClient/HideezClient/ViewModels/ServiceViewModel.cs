using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;

namespace HideezClient.ViewModels
{
    class ServiceViewModel : ObservableObject
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMetaPubSub _metaMessenger;

        public bool IsServiceConnected
        {
            get 
            { 
                return _serviceProxy.IsConnected; 
            }
        }

        public ServiceViewModel(IServiceProxy serviceProxy, IMetaPubSub metaMessenger)
        {
            _serviceProxy = serviceProxy;
            _metaMessenger = metaMessenger;

            _metaMessenger.Subscribe<ConnectionServiceChangedMessage>(OnServiceConnectionStateChanged);
        }

        Task OnServiceConnectionStateChanged(ConnectionServiceChangedMessage obj)
        {
            NotifyPropertyChanged(nameof(IsServiceConnected));

            return Task.CompletedTask;
        }
    }
}
