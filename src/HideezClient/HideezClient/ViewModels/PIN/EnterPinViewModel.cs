using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HideezClient.Extension;
using HideezClient.Models;
using HideezClient.Modules;
using HideezClient.Modules.Localize;

namespace HideezClient.ViewModels
{
    public class EnterPinViewModel : PinViewModelBase
    {
        private int nAttempts = 0;
        private SecureString enterPin;

        public EnterPinViewModel(IWindowsManager windowsManager) : base(windowsManager)
        {
            Type = PinViewType.Enter;
        }

        #region Properties

        public SecureString EnterPin
        {
            get { return enterPin; }
            set { Set(ref enterPin, value); }
        }

        public int NAttempts
        {
            get { return nAttempts; }
            private set { Set(ref nAttempts, value); }
        }

        #endregion Properties

        protected override async Task OnConfirmAsync()
        {
            cancelTokenSource = new CancellationTokenSource();
            forceValidate = true;

            if (!IsValidLenght(EnterPin, out string error))
            {
                RaisePropertyChanged(nameof(EnterPin));
                return;
            }

            State = ViewPinState.Progress;
            byte[] utf8Bytes = null;
            PinOperation operation = PinOperation.Unknown;
            try
            {
                utf8Bytes = EnterPin.ToUtf8Bytes();
                operation = await Device?.VerifyPinAsync(utf8Bytes, cancelTokenSource.Token);
            }
            finally
            {
                // clear byte pin array
                if (utf8Bytes != null)
                {
                    for (int i = 0; i < utf8Bytes.Length; i++)
                    {
                        utf8Bytes[i] = 0;
                    }
                }
            }

            if (State != ViewPinState.Canceled)
            {
                switch (operation)
                {
                    case PinOperation.Error:
                        NAttempts = Device.PinAttemptsRemain;
                        State = ViewPinState.WaitUserAction;
                        break;
                    case PinOperation.Successful:
                        windowsManager.ShowInfo(TranslationSource.Instance["Pin.ConfirmedPIN"]);
                        if (CloseIfSuccess)
                        {
                            windowsManager.CloseWindow(ID);
                        }
                        State = ViewPinState.Successful;
                        break;
                    case PinOperation.AccessDenied:
                        Task t = windowsManager.ShowDeviceLockedAsync();
                        windowsManager.CloseWindow(ID);
                        break;
                    case PinOperation.Canceled:
                        windowsManager.CloseWindow(ID);
                        break;
                }
            }
        }

        protected override string Validate(string memberName)
        {
            string error = null;

            if (memberName != null && memberName == nameof(EnterPin))
            {
                IsValidLenght(EnterPin, out error);
            }

            return error;
        }
    }
}
