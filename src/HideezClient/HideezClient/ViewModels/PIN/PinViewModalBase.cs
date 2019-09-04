using HideezClient.Extension;
using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.Localize;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    public enum ViewPinState
    {
        None,
        WaitUserAction,
        Progress,
        Successful,
        Canceled,
        AccessDenied,
        Error,
    }

    public enum PinViewType
    {
        Enter,
        Set,
        Change,
    }


    public abstract class PinViewModelBase : ObservableObject, IDataErrorInfo
    {
        private PinViewType type;
        private Device device;
        private uint minLenghtPin = 1;
        private uint maxLenghtPin = 8;
        private ViewPinState state;
        protected bool forceValidate = false;
        protected readonly IWindowsManager windowsManager;
        protected CancellationTokenSource cancelTokenSource;

        protected PinViewModelBase(IWindowsManager windowsManager)
        {
            this.windowsManager = windowsManager;
        }

        #region Properties

        string IDataErrorInfo.Error { get { return Validate(null); } }
        string IDataErrorInfo.this[string columnName] { get { return Validate(columnName); } }

        public Device Device
        {
            get { return device; }
            set { Set(ref device, value); }
        }

        [DependsOn(nameof(Device))]
        public string DeviceSN
        {
            get { return device?.SerialNo; }
        }

        public ViewPinState State
        {
            get { return state; }
            set { Set(ref state, value); }
        }

        public uint MaxLenghtPin
        {
            get { return maxLenghtPin; }
            set { Set(ref maxLenghtPin, value); }
        }

        public uint MinLenghtPin
        {
            get { return minLenghtPin; }
            set { Set(ref minLenghtPin, value); }
        }

        protected bool CloseIfSuccess { get; } = true;
        
        public PinViewType Type
        {
            get { return type; }
            set { Set(ref type, value); }
        }

        #endregion Properties

        #region Commands

        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnConfirmAsync(),
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

        #endregion Commands

        protected abstract Task OnConfirmAsync();
        protected abstract string Validate(string memberName);

        protected virtual void OnCancel()
        {
            if (State == ViewPinState.WaitUserAction || State == ViewPinState.Progress)
            {
                cancelTokenSource?.Cancel();
                State = ViewPinState.Canceled;
            }
        }

        protected virtual bool IsValidLenght(SecureString pin, out string error)
        {
            error = null;
            bool isValid = false;

            if (pin != null || forceValidate)
            {
                if (pin == null || pin.Length == 0)
                {
                    error = TranslationSource.Instance["Pin.Error.NotSpecified"];
                }
                else if (pin.Length < MinLenghtPin)
                {
                    error = string.Format(TranslationSource.Instance["Pin.Error.MinLenght"], MinLenghtPin);
                }
                else if (pin.Length > MaxLenghtPin)
                {
                    error = string.Format(TranslationSource.Instance["Pin.Error.MaxLenght"], MaxLenghtPin);
                }
                else
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        protected bool IsValidConfirmPin(SecureString pin, SecureString confirmPin, out string error)
        {
            error = null;
            bool isValid = ((pin == null && confirmPin == null) 
                || (pin != null && pin.Length == 0 && confirmPin == null)
                || (pin.Length == 0 && confirmPin.Length == 0)
                || (pin != null && confirmPin == null && !forceValidate));

            if (!isValid)
            {
                isValid = pin.IsEqualTo(confirmPin);
                if (!isValid)
                {
                    error = TranslationSource.Instance["Pin.Error.ConfirmNotMatch"];
                }
            }

            return isValid;
        }
    }
}
