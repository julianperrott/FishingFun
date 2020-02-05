using System;
using System.Drawing;

namespace FishingFun
{
    public class BobberColourPointFinder : IBobberFinder, IImageProvider
    {
        private Color targetColor;
        private Bitmap bmp = new Bitmap(1, 1);

        public BobberColourPointFinder(Color targetColor)
        {
            this.targetColor = targetColor;
            BitmapEvent += (s, e) => { };
        }

        public event EventHandler<BobberBitmapEvent> BitmapEvent;

        public Point Find()
        {
            this.bmp = WowScreen.GetBitmap();

            const int targetOffset = 15;

            var widthLower = 0;
            var widthHigher = bmp.Width;
            var heightLower = 0;
            var heightHigher = bmp.Height;

            var targetRedLb = targetColor.R - targetOffset;
            var targetRedHb = targetColor.R + targetOffset;
            var targetBlueLb = targetColor.B - targetOffset;
            var targetBlueHb = targetColor.B + targetOffset;
            var targetGreenLb = targetColor.G - targetOffset;
            var targetGreenHb = targetColor.G + targetOffset;

            var pos = new Point(0, 0);

            for (int i = widthLower; i < widthHigher; i++)
            {
                for (int j = heightLower; j < heightHigher; j++)
                {
                    pos.X = i;
                    pos.Y = j;
                    var colorAt = WowScreen.GetColorAt(pos, bmp);
                    if (colorAt.R > targetRedLb &&
                        colorAt.R < targetRedHb &&
                        colorAt.B > targetBlueLb &&
                        colorAt.B < targetBlueHb &&
                        colorAt.G > targetGreenLb &&
                        colorAt.G < targetGreenHb)
                    {
                        BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = new Point(i, j), Bitmap = bmp });
                        return WowScreen.GetScreenPositionFromBitmapPostion(pos);
                    }
                }
            }

            BitmapEvent?.Invoke(this, new BobberBitmapEvent { Point = Point.Empty, Bitmap = bmp });
            bmp.Dispose();
            return Point.Empty;
        }

        public Bitmap GetBitmap()
        {
            return bmp;
        }

        public void Reset()
        {
        }
    }
}