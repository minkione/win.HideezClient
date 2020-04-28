using System.Drawing;
using ZXing;
using ZXing.Common;

namespace HideezMiddleware.Utils
{
    public class QrBitmapFactory
    {
        /// <summary>
        /// Generate rectangular QR bitmap
        /// </summary>
        /// <param name="content">Content encoded into QR bitmap</param>
        /// <param name="width">Bitmap width</param>
        /// <param name="height">Bitmap height</param>
        /// <param name="margin">Margin between bitmap border and QR code</param>
        /// <returns></returns>
        public Bitmap GenerateQrBitmap(string content, int width, int height, int margin)
        {
            var barcodeWriter = new BarcodeWriter()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions()
                {
                    Height = width,
                    Width = height,
                    Margin = margin,
                },
            };

            var qrBitmap = barcodeWriter.Write(content);

            return qrBitmap;
        }

        /// <summary>
        /// Generates square QR bitmap
        /// </summary>
        /// <param name="content">Content encoded into QR bitmap</param>
        /// <param name="dimension">Bitmap side length</param>
        /// <param name="margin">Margin between bitmap border and QR code</param>
        public Bitmap GenerateQrBitmap(string content, int dimension, int margin)
        {
            return GenerateQrBitmap(content, dimension, dimension, margin);
        }
    }
}
