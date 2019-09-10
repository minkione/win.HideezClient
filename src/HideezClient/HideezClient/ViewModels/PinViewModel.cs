using GalaSoft.MvvmLight.Messaging;
using HideezClient.Extension;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using System.Security;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    public class PinViewModel : ObservableObject
    {
        readonly IMessenger _messenger;

        SecureString _secureCurrentPin;
        SecureString _secureNewPin;
        SecureString _secureConfirmPin;

        bool _askOldPin = false;
        bool _confirmNewPin = false;
        bool _inProgress = false;
        string _errorMessage = string.Empty;

        Device _device;
        uint _minLenghtPin = 1;
        uint _maxLenghtPin = 8;

        public PinViewModel(IMessenger messenger)
        {
            _messenger = messenger;

            RegisterDependencies();
        }

        #region Properties

        public SecureString SecureCurrentPin
        {
            get { return _secureCurrentPin; }
            set { Set(ref _secureCurrentPin, value); }
        }

        public SecureString SecureNewPin
        {
            get { return _secureNewPin; }
            set { Set(ref _secureNewPin, value); }
        }

        public SecureString SecureConfirmPin
        {
            get { return _secureConfirmPin; }
            set { Set(ref _secureConfirmPin, value); }
        }

        // Properties received from service
        public bool AskOldPin
        {
            get { return _askOldPin; }
            set { Set(ref _askOldPin, value); }
        }

        public bool ConfirmNewPin
        {
            get { return _confirmNewPin; }
            set { Set(ref _confirmNewPin, value); }
        }

        //...

        // Current PIN operation
        [DependsOn(nameof(AskOldPin), nameof(ConfirmNewPin))]
        public bool IsNewPin
        {
            get
            {
                return !AskOldPin && ConfirmNewPin;
            }
        }

        [DependsOn(nameof(AskOldPin), nameof(ConfirmNewPin))]
        public bool IsEnterPin
        {
            get
            {
                return !AskOldPin && !ConfirmNewPin;
            }
        }

        [DependsOn(nameof(AskOldPin), nameof(ConfirmNewPin))]
        public bool IsChangePin
        {
            get
            {
                return AskOldPin && ConfirmNewPin;
            }
        }
        //...

        // PasswordBox visibility fields
        [DependsOn(nameof(IsEnterPin), nameof(IsChangePin))]
        public bool AskCurrentPin
        {
            get
            {
                return IsEnterPin || IsChangePin;
            }
        }

        [DependsOn(nameof(IsNewPin), nameof(IsChangePin))]
        public bool AskNewPin
        {
            get
            {
                return IsNewPin || IsChangePin;
            }
        }

        [DependsOn(nameof(IsNewPin), nameof(IsChangePin))]
        public bool AskConfirmPin
        {
            get
            {
                return IsNewPin || IsChangePin;
            }
        }
        //...

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

        public uint MaxLenghtPin
        {
            get { return _maxLenghtPin; }
            set { Set(ref _maxLenghtPin, value); }
        }

        public uint MinLenghtPin
        {
            get { return _minLenghtPin; }
            set { Set(ref _minLenghtPin, value); }
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
        public int PinAttemptsLeft
        {
            get { return Device != null ? Device.PinAttemptsRemain : 0; }
        }

        #endregion Properties

        #region Commands
        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnConfirm(),
                    CanExecuteFunc = () => AreAllRequiredFieldsSet(),
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


        public void UpdateViewModel(Device device, bool askOldPin, bool confirmNewPin)
        {
            AskOldPin = askOldPin;
            ConfirmNewPin = confirmNewPin;

            ClearPasswordBoxes();

            ErrorMessage = string.Empty;
        }

        bool AreAllRequiredFieldsSet()
        {
            if (IsNewPin)
            {
                if (IsValidLength(SecureNewPin) != 0 || 
                    IsValidLength(SecureConfirmPin) != 0)
                    return false;
            }
            else if (IsEnterPin)
            {
                if (IsValidLength(SecureCurrentPin) != 0)
                    return false;
            }
            else if (IsChangePin)
            {
                if (IsValidLength(SecureNewPin) != 0 ||
                    IsValidLength(SecureCurrentPin) != 0 ||
                    IsValidLength(SecureConfirmPin) != 0)
                    return false;
            }

            return true;
        }

        void OnConfirm()
        {
            InProgress = true;
            ErrorMessage = string.Empty;

            // Gather data
            SecureString oldPin = null;
            SecureString pin = null;
            SecureString confirmPin = null;

            if (IsNewPin)
            {
                pin = SecureNewPin;
                confirmPin = SecureConfirmPin;
            }
            else if (IsEnterPin)
            {
                pin = SecureCurrentPin;
            }
            else if (IsChangePin)
            {
                oldPin = SecureCurrentPin;
                pin = SecureNewPin;
                confirmPin = SecureConfirmPin;
            }

            ClearPasswordBoxes();

            if (IsNewPin || IsChangePin)
            {
                if (IsConfirmPinCorrect(pin, confirmPin))
                {
                    ErrorMessage = "The new PIN and confirmation PIN does not match";
                    InProgress = false;
                    return;
                }
            }

            _messenger.Send(new SendPinMessage(Device.Id, pin.ToUtf8Bytes(), oldPin.ToUtf8Bytes()));
        }

        void OnCancel()
        {
            _messenger.Send(new HidePinUiMessage());
        }

        void ClearPasswordBoxes()
        {
            SecureCurrentPin.Clear();
            SecureNewPin.Clear();
            SecureConfirmPin.Clear();
        }

        /// <summary>
        /// Check if PIN is of sufficient length
        /// </summary>
        /// <returns>
        /// Returns 0 if pin is of sufficient length.
        /// Returns 1 if pin is to long.
        /// Returns -1 if pin is to short.
        /// </returns>
        int IsValidLength(SecureString pin)
        {
            if (pin.Length < MinLenghtPin)
                return -1;
            else if (pin.Length > MaxLenghtPin)
                return 1;
            
            return 0;
        }

        bool IsConfirmPinCorrect(SecureString pin, SecureString confirmPin)
        {
            return pin.IsEqualTo(confirmPin);
        }
    }
}
