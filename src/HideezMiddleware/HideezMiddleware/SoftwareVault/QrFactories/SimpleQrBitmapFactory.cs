using HideezMiddleware.Utils;
using System.Drawing;
using System.Text;

namespace HideezMiddleware.SoftwareVault.QrFactories
{
    public class SimpleQrBitmapFactory : QrBitmapFactory
    {
        public string Delimiter { get; set; }

        public int Dimension { get; set; }

        public int Margin { get; set; }

        public SimpleQrBitmapFactory(string delimiter, int dimension, int margin)
        {
            Delimiter = delimiter;
            Dimension = dimension;
            Margin = margin;
        }

        protected Bitmap GenerateQrBitmap(params string[] content)
        {
            var sb = new StringBuilder();

            foreach (var item in content)
                sb.Append(item).Append(Delimiter);

            return GenerateQrBitmap(sb.ToString(), Dimension, Margin);
        }
    }
}
