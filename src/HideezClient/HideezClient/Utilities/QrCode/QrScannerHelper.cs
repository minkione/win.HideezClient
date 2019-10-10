using System;
using System.Drawing;
using ZXing;

namespace HideezClient.Utilities.QrCode
{
    public class QrScannerHelper : IQrScannerHelper
    {
        private const string otpSecretParam = "secret=";
        readonly IBarcodeReader barcodeReader;

        public QrScannerHelper(IBarcodeReader barcodeReader)
        {
            this.barcodeReader = barcodeReader;
        }

        public Result DecoreQrFromImage(Bitmap bitmap)
        {
            return barcodeReader.Decode(bitmap);
        }

        public string GetOtpSecret(string scannedData)
        {
            string result;

            if (scannedData == null)
            {
                return string.Empty;
            }

            if (scannedData.Contains(otpSecretParam))
            {
                var start = scannedData.IndexOf(otpSecretParam, StringComparison.InvariantCultureIgnoreCase) + 7;
                var end = scannedData.IndexOf('&', start);

                if (start == scannedData.Length)
                {
                    result = scannedData;
                }
                else
                {
                    if (end <= start)
                        end = scannedData.Length;

                    string sicret = scannedData.Substring(start, end - start);
                    if (sicret == "&")
                    {
                        result = scannedData;
                    }
                    else
                    {
                        result = sicret;
                    }
                }
            }
            else
                result = scannedData;

            return result;
        }
    }
}
