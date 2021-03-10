using HideezClient.Messages.Dialogs.Wipe;
using HideezClient.Modules.DeviceManager;
using HideezClient.Mvvm;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Commands;
using System.Windows.Input;
using System.Linq;

namespace HideezClient.ViewModels.Dialog
{
    public class WipeViewModel : ObservableObject
    {
        readonly IMetaPubSub _messenger;
        readonly IDeviceManager _deviceManager;

        string _vaultName = string.Empty;
        string _deviceId = string.Empty;
        bool _inProgress = false;

        public WipeViewModel(IMetaPubSub messenger, IDeviceManager deviceManager)
        {
            _messenger = messenger;
            _deviceManager = deviceManager;
        }

        public string DeviceId
        {
            get { return _deviceId; }
            set { Set(ref _deviceId, value); }
        }

        public string VaultName
        {
            get { return _vaultName; }
            set { Set(ref _vaultName, value); }
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
            VaultName = _deviceManager.Devices.FirstOrDefault(d => d.Id == DeviceId)?.Name;
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
