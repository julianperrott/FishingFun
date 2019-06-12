using System;

namespace FishingFun
{
    public class PixelClassifier : IPixelClassifier
    {
        public double ColourMultiplier { get; set; } = 0.5;
        public double ColourClosenessMultiplier { get; set; } = 2.0;

        public bool IsMatch(byte red, byte green, byte blue)
        {
            return isBigger(red, green) && isBigger(red, blue) && areClose(blue, green);
        }

        private bool isBigger(byte red, byte other)
        {
            return (red * ColourMultiplier) > other;
        }

        private bool areClose(byte color1, byte color2)
        {
            var max = Math.Max(color1, color2);
            var min = Math.Min(color1, color2);

            return min * ColourClosenessMultiplier > max - 20;
        }
    }
}