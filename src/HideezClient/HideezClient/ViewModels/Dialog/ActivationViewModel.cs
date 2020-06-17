using GalaSoft.MvvmLight.Messaging;
using HideezClient.Mvvm;
using System;

namespace HideezClient.ViewModels
{
    public class ActivationViewModel : ObservableObject
    {
        readonly IMessenger _messenger;

        public ActivationViewModel(IMessenger messenger)
        {
            _messenger = messenger;

            RegisterDependencies();
        }

        public void Initialize(string deviceId)
        {
            throw new NotImplementedException();
        }

        public void UpdateViewModel(string deviceId)
        {
            throw new NotImplementedException();
        }
    }
}
