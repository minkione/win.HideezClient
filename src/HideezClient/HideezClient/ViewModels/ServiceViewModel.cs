using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;

namespace HideezClient.ViewModels
{
    class ServiceViewModel : ObservableObject
    {
        readonly IServiceProxy _serviceProxy;
        readonly IMessenger _messenger;

        public bool IsServiceConnected
        {
            get 
            { 
                return _serviceProxy.IsConnected; 
            }
        }

        public ServiceViewModel(IServiceProxy serviceProxy, IMessenger messenger)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;

            _messenger.Register<ConnectionServiceChangedMessage>(this, OnServiceConnectionStateChanged);
        }

        void OnServiceConnectionStateChanged(ConnectionServiceChangedMessage obj)
        {
            NotifyPropertyChanged(nameof(IsServiceConnected));
        }
    }
}
