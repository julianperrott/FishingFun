using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FishingFun
{
    public static class BitmapExtension
    {
        public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                memory.Dispose();
                return bitmapImage;
            }
        }

        public static Brush GetBackgroundColourBrush(this System.Drawing.Bitmap bitmap)
        {
            long r = 0, g = 0, b = 0;
            long pixels = bitmap.Width * bitmap.Height;

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                }
            }

            return new SolidColorBrush(Color.FromArgb(255, (byte)(r / pixels), (byte)(g / pixels), (byte)(b / pixels)));
        }
    }
}