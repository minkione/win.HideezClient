using Hideez.SDK.Communication.Workstation;
using System.Drawing;

namespace HideezMiddleware.SoftwareVault.QrFactories
{
    internal class UnlockQrBitmapFactory : SimpleQrBitmapFactory
    {
        const int QR_DIMENSION = 256;
        const int QR_MARGIN = 0;
        const string QR_DELIMITER = ";";

        readonly IWorkstationInfoProvider _workstationInfoProvider;

        public UnlockQrBitmapFactory(IWorkstationInfoProvider workstationInfoProvider)
            : base(QR_DELIMITER, QR_DIMENSION, QR_MARGIN)
        {
            _workstationInfoProvider = workstationInfoProvider;
        }

        public Bitmap GenerateUnlockQrBitmap(string unlockToken)
        {
            var workstationInfo = _workstationInfoProvider.GetWorkstationInfo();
            return GenerateQrBitmap("u", unlockToken, _workstationInfoProvider.WorkstationId, workstationInfo.MachineName);
        }
    }
}
