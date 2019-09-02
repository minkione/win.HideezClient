using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HideezClient.Extension;
using HideezClient.Modules;
using HideezClient.Modules.Localize;

namespace HideezClient.ViewModels
{
    public class SetPinViewModel : PinViewModelBase
    {
        private SecureString setPin;
        private SecureString setPinConfirm;

        public SetPinViewModel(IWindowsManager windowsManager) : base(windowsManager)
        {
            Type = PinViewType.Set;
        }

        #region Properties

        public SecureString SetPin
        {
            get { return setPin; }
            set { Set(ref setPin, value); }
        }

        public SecureString SetPinConfirm
        {
            get { return setPinConfirm; }
            set { Set(ref setPinConfirm, value); }
        }

        #endregion Properties

        protected override async Task OnConfirmAsync()
        {
            cancelTokenSource = new CancellationTokenSource();
            forceValidate = true;

            if (!IsValidLenght(SetPin, out string errorL))
            {
                RaisePropertyChanged(nameof(SetPin));
                return;
            }
            else if (!SetPin.IsEqualTo(SetPinConfirm))
            {
                RaisePropertyChanged(nameof(SetPinConfirm));
                return;
            }

            State = ViewPinState.Progress;
            byte[] pinBytes = null;
            try
            {
                pinBytes = SetPin.ToUtf8Bytes();
                var operation = await Device.SetPinAsync(pinBytes, cancelTokenSource.Token);

                if (State != ViewPinState.Canceled)
                {
                    switch (operation)
                    {
                        case Models.PinOperation.Successful:
                            windowsManager.ShowInfo(TranslationSource.Instance["Pin.SavedPIN"]);
                            if (!CloseIfSuccess)
                            {
                                State = ViewPinState.Successful;
                            }
                            break;
                        case Models.PinOperation.AccessDenied:
                            Task t = windowsManager.ShowDeviceLockedAsync();
                            break;
                        case Models.PinOperation.Unknown:
                        case Models.PinOperation.Error:
                            State = ViewPinState.Error;
                            windowsManager.ShowError(TranslationSource.Instance["Pin.ErrorSavedPIN"]);
                            break;
                    }
                }
            }
            finally
            {
                // clear byte pin array
                if (pinBytes != null)
                {
                    for (int i = 0; i < pinBytes.Length; i++)
                    {
                        pinBytes[i] = 0;
                    }
                }
            }

            windowsManager.CloseWindow(ID);
        }

        protected override string Validate(string memberName)
        {
            string error = null;

            if (memberName != null)
            {
                if (memberName == nameof(SetPin))
                {
                    IsValidLenght(SetPin, out error);
                    RaisePropertyChanged(nameof(SetPinConfirm));
                }
                else if (memberName == nameof(SetPinConfirm))
                {
                    IsValidConfirmPin(SetPin, SetPinConfirm, out error);
                }
            }

            return error;
        }
    }
}
