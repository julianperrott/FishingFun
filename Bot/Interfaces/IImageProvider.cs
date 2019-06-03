using System;
using System.Drawing;

namespace FishingFun
{
    public interface IImageProvider
    {
        event EventHandler<BobberBitmapEvent> BitmapEvent;
    }

    public class BobberBitmapEvent : EventArgs
    {
        public Bitmap Bitmap { get; set; }
        public Point Point { get; set; }
    }

    public class BobberMoveEvent : EventArgs
    {
        public int Amplitude;

        public bool Threshold;
    }
}