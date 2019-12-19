using System.Drawing;
using ZXing;

namespace HideezClient.Utilities.QrCode
{
    public interface IQrScannerHelper
    {
        Result DecoreQrFromImage(Bitmap bitmap);

        string GetOtpSecret(string data);

    }
}
