using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Modules.Localize;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Security;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class ServerAddressEditControlViewModel : ObservableObject, IDisposable
    {
        readonly IServiceProxy _serviceProxy;
        private readonly IMessenger _messenger;
        private readonly ILog _log;

        string _savedServerAddress;

        string _serverAddress;
        string _errorServerAddress;
        string _errorClarification; // Displayed alongside the error to provide additional information for possible solution
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

        public string ErrorClarification
        {
            get { return _errorClarification; }
            set { Set(ref _errorClarification, value); }
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
        public ServerAddressEditControlViewModel(IServiceProxy serviceProxy, IMessenger messenger, ILog log)
        {
            _serviceProxy = serviceProxy;
            _messenger = messenger;
            _log = log;

            _serviceProxy.Connected += ServiceProxy_Connected;

            Task.Run(InitializeViewModel).ConfigureAwait(false);
        }

        async void ServiceProxy_Connected(object sender, EventArgs e)
        {
            await InitializeViewModel();
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
                    var result = await _serviceProxy.GetService().ChangeServerAddressAsync(ServerAddress).ConfigureAwait(false);

                    switch (result)
                    {
                        case ChangeServerAddressResult.Success:
                            ResetViewModel(ServerAddress);
                            break;
                        case ChangeServerAddressResult.ConnectionTimedOut:
                            DisplayError("Error.CantReachServer", "Error.Clarification.ServerUnavailable");
                            break;
                        case ChangeServerAddressResult.KeyNotFound:
                            DisplayError("Error.CantSaveAddress", "Error.Clarification.KeyNotFound");
                            break;
                        case ChangeServerAddressResult.UnauthorizedAccess:
                            DisplayError("Error.CantSaveAddress", "Error.Clarification.UnauthorizedAccess");
                            break;
                        case ChangeServerAddressResult.SecurityError:
                            DisplayError("Error.CantSaveAddress", "Error.Clarification.SecurityError");
                            break;
                        case ChangeServerAddressResult.UnknownError:
                            DisplayError("Error.UnexpectedError", "Error.Clarification.UnexpectedError");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.WriteLine(nameof(ServerAddressEditControlViewModel), ex);
                    _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                }
                finally
                {
                    Interlocked.Exchange(ref _saving, 0);
                    CheckingConnection = false;
                }
            }
        }

        async Task InitializeViewModel()
        {
            try
            {
                if (_serviceProxy.IsConnected)
                {
                    var address = await _serviceProxy.GetService().GetServerAddressAsync();
                    ResetViewModel(address);
                }
            }
            catch (Exception) { }
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

        void DisplayError(string errorMessageKey, string clarificationMessageKey)
        {
            ErrorServerAddress = TranslationSource.Instance[errorMessageKey];
            ErrorClarification = TranslationSource.Instance[clarificationMessageKey];
            ShowError = true;
            ShowInfo = false;
        }

        #region IDisposable Support
        bool disposed = false;

        protected virtual void Dispose(bool dispose)
        {
            if (!disposed)
            {
                if (dispose)
                {
                    _serviceProxy.Connected -= ServiceProxy_Connected;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
