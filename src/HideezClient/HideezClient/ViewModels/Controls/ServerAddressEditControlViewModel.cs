using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Modules.Localize;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using MvvmExtensions.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class ServerAddressEditControlViewModel : ObservableObject
    {
        readonly IMetaPubSub _metaMessenger;
        readonly ILog _log;

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

        // TODO: Add error handling if service is offline
        public ServerAddressEditControlViewModel(IMetaPubSub metaMessenger, ILog log)
        {
            _metaMessenger = metaMessenger;
            _log = log;

            _metaMessenger.TrySubscribeOnServer<ServiceSettingsChangedMessage>(OnServiceSettingsChanged);
            try
            {
                _metaMessenger.PublishOnServer(new RefreshServiceInfoMessage());
            }
            catch (Exception) { } // Handle error in case we are not connected to server
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
                    var reply = await _metaMessenger.ProcessOnServer<ChangeServerAddressMessageReply>(new ChangeServerAddressMessage(ServerAddress), 0);

                    if (reply.ChangedSuccessfully)
                    {
                        // Reset will be carried out when we receive OnServiceSettingsChanged message
                        // ResetViewModel(ServerAddress);
                    }
                    else
                    {
                        OnAddressChangeFail();
                    }
                }
                catch (Exception ex)
                {
                    _log.WriteLine(nameof(ServerAddressEditControlViewModel), ex);
                    await _metaMessenger.Publish(new ShowErrorNotificationMessage(ex.Message));
                }
                finally
                {
                    Interlocked.Exchange(ref _saving, 0);
                    CheckingConnection = false;
                }
            }
        }

        Task OnServiceSettingsChanged(ServiceSettingsChangedMessage arg)
        {
            ResetViewModel(arg.ServerAddress);
            return Task.CompletedTask;
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
