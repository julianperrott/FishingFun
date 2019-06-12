using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FishingFun
{
    public partial class ColourConfiguration : System.Windows.Window
    {
        private readonly IPixelClassifier pixelClassifier;

        private Bitmap ScreenCapture;

        public int RedValue { get; set; }

        public int ColourMultiplier
        {
            get
            {
                return (int)(pixelClassifier.ColourMultiplier * 100);
            }
            set
            {
                pixelClassifier.ColourMultiplier = ((double)value) / 100;
            }
        }

        public int ColourClosenessMultiplier
        {
            get
            {
                return (int)(pixelClassifier.ColourClosenessMultiplier * 100);
            }
            set
            {
                pixelClassifier.ColourClosenessMultiplier = ((double)value) / 100;
            }
        }

        public ColourConfiguration(IPixelClassifier pixelClassifier)
        {
            this.pixelClassifier = pixelClassifier;
            RedValue = 100;

            InitializeComponent();

            this.DataContext = this;
        }

        private void RenderColour(bool renderMatchedArea)
        {
            var bitmap = new System.Drawing.Bitmap(256, 256);

            var points = new List<Point>();

            for (var b = 0; b < 256; b++)
            {
                for (var g = 0; g < 256; g++)
                {
                    if (pixelClassifier.IsMatch((byte)this.RedValue, (byte)g, (byte)b))
                    {
                        points.Add(new Point(b, g));
                    }
                    bitmap.SetPixel(b, g, System.Drawing.Color.FromArgb(this.RedValue, g, b));
                }
            }

            if (ScreenCapture == null)
            {
                ScreenCapture = WowScreen.GetBitmap();
                renderMatchedArea = true;
            }

            this.ColourDisplay.Source = bitmap.ToBitmapImage();
            this.WowScreenshot.Source = ScreenCapture.ToBitmapImage();

            if (renderMatchedArea)
            {
                Dispatch(() =>
                {
                    MarkEdgeOfRedArea(bitmap, points);
                    this.ColourDisplay.Source = bitmap.ToBitmapImage();
                });

                Dispatch(() =>
                {
                    Bitmap bmp = new Bitmap(ScreenCapture);
                    MarkRedOnBitmap(bmp);
                    this.WowScreenshot.Source = bmp.ToBitmapImage();
                });
            }
        }

        private void MarkRedOnBitmap(Bitmap bmp)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    if (this.pixelClassifier.IsMatch(pixel.R, pixel.G, pixel.B))
                    {
                        bmp.SetPixel(x, y, Color.Red);
                    }
                }
            }
        }

        private static void MarkEdgeOfRedArea(Bitmap bitmap, List<Point> points)
        {
            foreach (var point in points)
            {
                var pointsClose = points.Count(p => (p.X == point.X && (p.Y == point.Y - 1 || p.Y == point.Y + 1)) || (p.Y == point.Y && (p.X == point.X - 1 || p.X == point.X + 1)));
                if (pointsClose < 4)
                {
                    bitmap.SetPixel(point.X, point.Y, Color.White);
                }
            }
        }

        private void Red_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            this.LabelRed.Content = this.RedValue;
            RenderColour(false);
        }

        private void ColourMultiplier_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            this.LabelColourMultiplier.Text = $"Red multiplied by {this.pixelClassifier.ColourMultiplier} must be greater than green and blue.";
        }

        private void ColourClosenessMultiplier_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            this.LabelColourClosenessMultiplier.Text = $"How close Green and Blue need to be to each other: {this.pixelClassifier.ColourClosenessMultiplier}";
        }

        private void Slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            RenderColour(true);
        }

        private void Capture_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ScreenCapture = WowScreen.GetBitmap();
            RenderColour(true);
        }

        private void Dispatch(Action action)
        {
            System.Windows.Application.Current?.Dispatcher.BeginInvoke((Action)(() => action()));
            System.Windows.Application.Current?.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}