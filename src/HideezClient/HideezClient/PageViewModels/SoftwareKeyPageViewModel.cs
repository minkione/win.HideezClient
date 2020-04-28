using Hideez.SDK.Communication.Workstation;
using HideezClient.Extension;
using HideezClient.Mvvm;
using HideezMiddleware.SoftwareVault.QrFactories;
using System.Windows;
using System.Windows.Media.Imaging;

namespace HideezClient.PageViewModels
{
    class SoftwareKeyPageViewModel : LocalizedObject
    {
        readonly ActivationQrBitmapFactory _activationQrBitmapFactory;
        BitmapImage _activationQR;

        public BitmapImage ActivationQR
        {
            get { return _activationQR; }
            set { Set(ref _activationQR, value); }
        }

        public SoftwareKeyPageViewModel(IWorkstationInfoProvider workstationInfoProvider)
        {
            _activationQrBitmapFactory = new ActivationQrBitmapFactory(workstationInfoProvider);

            GenerateActivationQr();
        }

        void GenerateActivationQr()
        {
            var qrBitmap = _activationQrBitmapFactory.GenerateActivationQrBitmap();
            Application.Current.Dispatcher.Invoke(() => { ActivationQR = qrBitmap.ToBitmapImage(); });
        }
    }
}
