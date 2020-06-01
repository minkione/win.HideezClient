using Hideez.SDK.Communication.Workstation;
using System.Drawing;

namespace HideezMiddleware.SoftwareVault.QrFactories
{
    public class ActivationQrBitmapFactory : SimpleQrBitmapFactory
    {
        const int QR_DIMENSION = 152;
        const int QR_MARGIN = 0;
        const string QR_DELIMITER = ";";

        readonly IWorkstationInfoProvider _workstationInfoProvider;

        public ActivationQrBitmapFactory(IWorkstationInfoProvider workstationInfoProvider)
            : base(QR_DELIMITER, QR_DIMENSION, QR_MARGIN)
        {
            _workstationInfoProvider = workstationInfoProvider;
        }

        public Bitmap GenerateActivationQrBitmap()
        {
            var workstationInfo = _workstationInfoProvider.GetWorkstationInfo();
            return GenerateQrBitmap("s", _workstationInfoProvider.WorkstationId, workstationInfo.MachineName);
        }
    }
}
