using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Interfaces;
using HideezClient.Extension;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Modules.DeviceManager;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using System;
using System.Linq;
using System.Security;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    public class ActivationViewModel : ObservableObject
    {
        readonly IMessenger _messenger;
        readonly IDeviceManager _deviceManager;
        readonly IMetaPubSub _metaMessenger;
        readonly byte[] _emptyBytes = new byte[0];
        readonly object initLock = new object();

        SecureString _secureActivationCode;
        bool _inProgress = false;
        string _errorMessage = string.Empty;

        Device _device;

        public event EventHandler ViewModelUpdated;
        public event EventHandler PasswordsCleared;

        public ActivationViewModel(IMessenger messenger, IMetaPubSub metaMessenger, IDeviceManager deviceManager)
        {
            _messenger = messenger;
            _deviceManager = deviceManager;
            _metaMessenger = metaMessenger;

            RegisterDependencies();
        }

        #region Properties
        public SecureString SecureActivationCode
        {
            get { return _secureActivationCode; }
            set { Set(ref _secureActivationCode, value); }
        }

        public bool InProgress
        {
            get { return _inProgress; }
            set { Set(ref _inProgress, value); }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { Set(ref _errorMessage, value); }
        }

        public int MaxCodeLength
        {
            get { return 8; }
        }

        public int MinCodeLength
        {
            get
            {
                return 6;
            }
        }

        public Device Device
        {
            get { return _device; }
            set { Set(ref _device, value); }
        }

        [DependsOn(nameof(Device))]
        public string SerialNo
        {
            get { return _device?.SerialNo; }
        }

        [DependsOn(nameof(Device))]
        public int AttemptsRemain
        {
            get { return Device != null ? Device.UnlockAttemptsRemain : 0; }
        }
        #endregion

        #region Commands
        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnConfirm(),
                    CanExecuteFunc = () => AreAllRequiredFieldsSet() && !InProgress,
                };
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnCancel();
                    },
                };
            }
        }
        #endregion


        public void Initialize(string deviceId)
        {
            lock (initLock)
            {
                if (Device == null && !string.IsNullOrWhiteSpace(deviceId))
                {
                    var device = _deviceManager.Devices.FirstOrDefault(d => d.Id == deviceId);
                    if (device != null)
                    {
                        Device = device;
                        Device.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
                    }

                    ResetProgress();
                }
            }
        }

        public void UpdateViewModel(string deviceId)
        {
            if (Device?.Id != deviceId)
                return;

            ResetProgress();
            ViewModelUpdated?.Invoke(this, EventArgs.Empty);
        }

        bool AreAllRequiredFieldsSet()
        {
            if (IsValidLength(SecureActivationCode, MinCodeLength, MaxCodeLength) != 0)
                return false;

            return true;
        }

        void OnConfirm()
        {
            InProgress = true;
            ErrorMessage = string.Empty;

            // Gather data
            SecureString code = SecureActivationCode;

            var codeBytes = code != null ? code.ToUtf8Bytes() : _emptyBytes;

            _messenger.Send(new SendActivationCodeMessage(Device.Id, codeBytes));

            ClearPasswords();
        }

        void OnCancel()
        {
            _messenger.Send(new CancelActivationCodeEntryMessage(Device.Id));
            _metaMessenger.PublishOnServer(new HideActivationCodeUi());
            Device.CancelDeviceAuthorization();
        }

        void ClearPasswords()
        {
            SecureActivationCode?.Clear();
            PasswordsCleared?.Invoke(this, EventArgs.Empty);
        }

        void ResetProgress()
        {
            ClearPasswords();
            InProgress = false;
        }

        // Todo: Move to the general Untils class
        /// <summary>
        /// Check if code in secure string is of sufficient length
        /// </summary>
        /// <returns>
        /// Returns 0 if code is of sufficient length.
        /// Returns 1 if code is to long.
        /// Returns -1 if code is to short.
        /// </returns>
        int IsValidLength(SecureString code, int minLength, int maxLengh)
        {
            if (code == null)
                return -1;

            if (code.Length < minLength)
                return -1;
            else if (code.Length > maxLengh)
                return 1;

            return 0;
        }
    }
}
