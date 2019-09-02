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
    public class ChangePinViewModel : PinViewModelBase
    {
        private SecureString changePinOld;
        private SecureString changePinNew;
        private SecureString changePinConfirm;

        public ChangePinViewModel(IWindowsManager windowsManager) : base(windowsManager)
        {
            Type = PinViewType.Set;
        }

        #region Properties

        public SecureString ChangePinOld
        {
            get { return changePinOld; }
            set { Set(ref changePinOld, value); }
        }

        public SecureString ChangePinNew
        {
            get { return changePinNew; }
            set { Set(ref changePinNew, value); }
        }

        public SecureString ChangePinConfirm
        {
            get { return changePinConfirm; }
            set { Set(ref changePinConfirm, value); }
        }

        #endregion Properties

        protected override async Task OnConfirmAsync()
        {
            cancelTokenSource = new CancellationTokenSource();
            forceValidate = true;

            if (!IsValidLenght(ChangePinOld, out string errorL1))
            {
                RaisePropertyChanged(nameof(ChangePinOld));
                return;
            }
            else if (!IsValidLenght(ChangePinNew, out string errorL2))
            {
                RaisePropertyChanged(nameof(ChangePinNew));
                return;
            }
            else if (!ChangePinNew.IsEqualTo(ChangePinConfirm))
            {
                RaisePropertyChanged(nameof(ChangePinConfirm));
                return;
            }

            State = ViewPinState.Progress;
            byte[] newPinBytes = null;
            byte[] oldPinBytes = null;
            try
            {
                newPinBytes = ChangePinNew.ToUtf8Bytes();
                oldPinBytes = ChangePinOld.ToUtf8Bytes();
                var operation = await Device.ChangePin(oldPinBytes, newPinBytes, cancelTokenSource.Token);
                if (State != ViewPinState.Canceled)
                {
                    switch (operation)
                    {
                        case Models.PinOperation.Successful:
                            windowsManager.ShowInfo(TranslationSource.Instance["Pin.ChangedPIN"]);
                            if (!CloseIfSuccess)
                            {
                                State = ViewPinState.Successful;
                            }
                            break;
                        case Models.PinOperation.Canceled:
                        default:
                            break;
                        case Models.PinOperation.AccessDenied:
                            Task t = windowsManager.ShowDeviceLockedAsync();
                            break;
                        case Models.PinOperation.Unknown:
                        case Models.PinOperation.Error:
                            windowsManager.ShowError(TranslationSource.Instance["Pin.ErrorChangePIN"]);
                            State = ViewPinState.Error;
                            break;
                    }
                }
            }
            finally
            {
                if (newPinBytes != null)
                {
                    for (int i = 0; i < newPinBytes.Length; i++)
                    {
                        newPinBytes[i] = 0;
                    }
                }

                if (oldPinBytes != null)
                {
                    for (int i = 0; i < oldPinBytes.Length; i++)
                    {
                        oldPinBytes[i] = 0;
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
                if (memberName == nameof(ChangePinOld))
                {
                    IsValidLenght(ChangePinOld, out error);
                }
                else if (memberName == nameof(ChangePinNew))
                {
                    IsValidLenght(ChangePinNew, out error);
                    RaisePropertyChanged(nameof(ChangePinConfirm));
                }
                else if (memberName == nameof(ChangePinConfirm))
                {
                    IsValidConfirmPin(ChangePinNew, changePinConfirm, out error);
                }
            }

            return error;
        }
    }
}
