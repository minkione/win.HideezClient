using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace HideezClient.Extension
{
    internal static class BitmapExtension
    {
        // credit to Gerret, https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            MemoryStream memory = new MemoryStream();

            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            if (bitmapImage.CanFreeze)
                bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
