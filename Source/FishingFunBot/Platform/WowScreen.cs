using System.Drawing;
using System.Windows.Forms;

namespace FishingFun
{
    public static class WowScreen
    {
        public static Color GetColorAt(Point pos, Bitmap bmp)
        {
            return bmp.GetPixel(pos.X, pos.Y);
        }

        public static Bitmap GetBitmap()
        {
            var bmpScreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width / 2, (Screen.PrimaryScreen.Bounds.Height / 2)-100);
            var graphics = Graphics.FromImage(bmpScreen);
            graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.Width / 4, Screen.PrimaryScreen.Bounds.Height / 4, 0, 0, bmpScreen.Size);
            graphics.Dispose();
            return bmpScreen;
        }

        public static Point GetScreenPositionFromBitmapPostion(Point pos)
        {
            return new Point(pos.X += Screen.PrimaryScreen.Bounds.Width / 4, pos.Y += Screen.PrimaryScreen.Bounds.Height / 4);
        }
    }
}