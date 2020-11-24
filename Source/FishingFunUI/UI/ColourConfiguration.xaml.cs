using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace FishingFun
{
    public partial class ColourConfiguration : System.Windows.Window
    {
        private readonly IPixelClassifier pixelClassifier;

        private Bitmap ScreenCapture = new Bitmap(1, 1);

        public int FindColourValue { get; set; }

        public string PrimaryColor = "Red";
        public string SecondaryColor = "blue";

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
            FindColourValue = 100;

            InitializeComponent();

            this.DataContext = this;

            cmbColors.ItemsSource = typeof(System.Windows.Media.Colors).GetProperties().Where(p => new List<string> { "Red", "Blue" }.Contains(p.Name));
            cmbColors.SelectedIndex = this.pixelClassifier.Mode==PixelClassifier.ClassifierMode.Blue?0: 1;
            LootDelay.Value = WowProcess.LootDelay;
        }

        private void RenderColour(bool renderMatchedArea)
        {
            var bitmap = new System.Drawing.Bitmap(256, 256);

            var points = new List<Point>();

            for (var i = 0; i < 256; i++)
            {
                for (var g = 0; g < 256; g++)
                {
                    var r = (byte)this.FindColourValue;
                    var b = (byte)i;

                    if (this.pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue)
                    {
                        r = (byte)i;
                        b = (byte)this.FindColourValue;

                    }

                    if (pixelClassifier.IsMatch(r, (byte)g, b))
                    {
                        points.Add(new Point(i, g));
                    }
                    bitmap.SetPixel(i, g, Color.FromArgb(r, g, b));

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
                    MarkHighlightOnBitmap(bmp);
                    this.WowScreenshot.Source = bmp.ToBitmapImage();
                });
            }
        }

        private void MarkHighlightOnBitmap(Bitmap bmp)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    var pixel = bmp.GetPixel(x, y);
                    if (this.pixelClassifier.IsMatch(pixel.R, pixel.G, pixel.B))
                    {
                        bmp.SetPixel(x, y, this.pixelClassifier.Mode == PixelClassifier.ClassifierMode.Blue ? Color.Blue : Color.Red);
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
        private void LootDelay_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            WowProcess.LootDelay = (int)this.LootDelay.Value;
        }

        private void FindColour_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            this.LabelRed.Content = this.FindColourValue;
            RenderColour(false);
        }

        private void ColourMultiplier_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColourText();
        }

        private void ColourClosenessMultiplier_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColourText();
        }

        public void UpdateColourText()
        {
            this.LabelColourMultiplier.Text = $"{PrimaryColor} multiplied by {this.pixelClassifier.ColourMultiplier} must be greater than green and {SecondaryColor}.";
            this.LabelColourClosenessMultiplier.Text = $"How close green and {SecondaryColor} need to be to each other: {this.pixelClassifier.ColourClosenessMultiplier}";
            this.ColourLabel.Content = PrimaryColor + ":";
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

        private void cmbColors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cmbColors.SelectedItem != null)
            {
                var propertyInfo = cmbColors.SelectedItem as PropertyInfo;
                var item = propertyInfo?.GetValue(null, null);
                if (item != null)
                {
                    var selectedColor = (System.Windows.Media.Color)item;

                    if (propertyInfo?.Name == "Red")
                    {
                        PrimaryColor = "Red";
                        SecondaryColor = "blue";
                        this.pixelClassifier.Mode = PixelClassifier.ClassifierMode.Red;
                    }
                    else
                    {
                        PrimaryColor = "Blue";
                        SecondaryColor = "red";
                        this.pixelClassifier.Mode = PixelClassifier.ClassifierMode.Blue;
                    }

                    UpdateColourText();
                    RenderColour(true);
                }
            }
        }
    }
}