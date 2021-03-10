using HideezClient.Messages.Dialogs.Wipe;
using HideezClient.Mvvm;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Commands;
using System.Windows.Input;

namespace HideezClient.ViewModels.Dialog
{
    public class WipeViewModel : ObservableObject
    {
        readonly IMetaPubSub _messenger;

        string _deviceId = string.Empty;
        bool _inProgress = false;

        public WipeViewModel(IMetaPubSub messenger)
        {
            _messenger = messenger;
        }

        public string DeviceId
        {
            get { return _deviceId; }
            set { Set(ref _deviceId, value); }
        }

        public bool InProgress
        {
            get { return _inProgress; }
            set { Set(ref _inProgress, value); }
        }

        public ICommand CancelWipeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnCancelWipe();
                    }
                };
            }
        }

        public ICommand StartWipeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnStartWipe();
                    }
                };
            }
        }

        public void Initialize(string deviceId)
        {
            DeviceId = deviceId;
        }

        void OnCancelWipe()
        {
            _messenger.Publish(new CancelWipeMessage(DeviceId));
        }

        void OnStartWipe()
        {
            InProgress = true;
            _messenger.Publish(new StartWipeMessage(DeviceId));
        }
    }
}
