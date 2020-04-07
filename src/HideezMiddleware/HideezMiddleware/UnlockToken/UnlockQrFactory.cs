using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using System.Drawing;
using System.Text;
using ZXing;
using ZXing.Common;

namespace HideezMiddleware.UnlockToken
{
    internal class UnlockQrFactory : Logger
    {
        const int QR_DIMENSION = 256;
        const int QR_MARGIN = 0;
        const string QR_DELIMITER = ";";

        readonly IWorkstationInfoProvider _workstationInfoProvider;

        public UnlockQrFactory(IWorkstationInfoProvider workstationInfoProvider, ILog log)
            : base(nameof(UnlockQrFactory), log)
        {
            _workstationInfoProvider = workstationInfoProvider;
        }

        public Bitmap GenerateNewUnlockQr(string unlockToken)
        {
            var workstationInfo = _workstationInfoProvider.GetWorkstationInfo();
            var content = GenerateUnlockQrContent(unlockToken, _workstationInfoProvider.WorkstationId, workstationInfo.MachineName);

            var barcodeWriter = new BarcodeWriter() 
            { 
                Format = BarcodeFormat.QR_CODE, 
                Options = new EncodingOptions() 
                {
                    Height = QR_DIMENSION,
                    Width = QR_DIMENSION,
                    Margin = QR_MARGIN,
                },
            };

            var qrBitmap = barcodeWriter.Write(content);

            return qrBitmap;
        }

        string GenerateUnlockQrContent(string unlockToken, string workstationId, string workstationName)
        {
            var sb = new StringBuilder();
            sb.Append("u" + QR_DELIMITER);
            sb.Append(unlockToken + QR_DELIMITER);
            sb.Append(workstationId + QR_DELIMITER);
            sb.Append(workstationName + QR_DELIMITER);

            return sb.ToString();
        }
    }
}
