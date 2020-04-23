using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Modules.Localize;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class SaveAddressEditControlViewModel : ObservableObject
    {
        readonly IServiceProxy _serviceProxy;
        private readonly IMessenger _messenger;
        private readonly ILog _log;

        string _savedServerAddress;

        string _serverAddress;
        string _errorServerAddress;
        bool _hasChanges = false;

        bool _showInfo;
        bool _showError;

        bool _checkingConnection;
        int _saving = 0;

        #region Properties
        public string ServerAddress
        {
            get { return _serverAddress; }
            set 
            {
                if (_serverAddress != value)
                {
                    _serverAddress = value;
                    NotifyPropertyChanged();
                    HasChanges = true; 
                }
            }
        }

        public string ErrorServerAddress
        {
            get { return _errorServerAddress; }
            set { Set(ref _errorServerAddress, value); }
        }

        public bool HasChanges
        {
            get { return _hasChanges; }
            set { Set(ref _hasChanges, value); }
        }

        public bool ShowInfo
        {
            get { return _showInfo; }
            set { Set(ref _showInfo, value); }
        }

        public bool ShowError
        {
            get { return _showError; }
            set { Set(ref _showError, value); }
        }

        public bool CheckingConnection
        {
            get { return _checkingConnection; }
            set { Set(ref _checkingConnection, value); }
        }
        #endregion

        #region Commands
        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => HasChanges && _savedServerAddress != ServerAddress && !CheckingConnection,
                    CommandAction = (x) => 
                    {
                        OnCancel();
                    }
                };
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => HasChanges && _savedServerAddress != ServerAddress && !CheckingConnection,
                    CommandAction = (x) =>
                    {
                        OnSave();
                    }
                };
            }
        }
        #endregion

        // TODO: Add re-initialization if service is reconnected
        // TODO: Add error handling if service is offline
        public SaveAddressEditControlViewModel(IServiceProxy serviceProxy, IMessenger messenger, ILog log)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
            _log = log;

            Task.Run(InitializeViewModel).ConfigureAwait(false);
        }

        void OnCancel()
        {
            ResetViewModel(_savedServerAddress);
        }

        async void OnSave()
        {
            if (Interlocked.CompareExchange(ref _saving, 1, 0) == 0)
            {
                CheckingConnection = true;
                ErrorServerAddress = null;
                try
                {
                    var success = await _serviceProxy.GetService().ChangeServerAddressAsync(ServerAddress).ConfigureAwait(false);

                    if (success)
                    {
                        ResetViewModel(ServerAddress);
                    }
                    else
                    {
                        OnAddressChangeFail();
                    }
                }
                catch (Exception ex)
                {
                    _log.WriteLine(nameof(SaveAddressEditControlViewModel), ex);
                    _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                }
                finally
                {
                    Interlocked.Exchange(ref _saving, 0);
                    CheckingConnection = false;
                }
            }
        }

        public async Task InitializeViewModel()
        {
            var address = await _serviceProxy.GetService().GetServerAddressAsync();
            ResetViewModel(address);
        }

        void ResetViewModel(string address)
        {
            _savedServerAddress = address;
            ServerAddress = address;
            ErrorServerAddress = null;

            if (string.IsNullOrWhiteSpace(address))
            {
                ShowInfo = true;
                ShowError = false;
            }
            else
            {
                ShowInfo = false;
                ShowError = false;
            }

            HasChanges = false;
        }

        void OnAddressChangeFail()
        {
            ErrorServerAddress = TranslationSource.Instance["Error.CantReachServer"];
            ShowError = true;
            ShowInfo = false;
        }
    }
}
