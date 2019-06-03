using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FishingFun
{
    public partial class MainWindow : Window, IAppender
    {
        private int index;
        private System.Drawing.Point lastPoint = System.Drawing.Point.Empty;

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        private SearchBobberFinder bobberFinder;
        private IPixelClassifier pixelClassifier;
        private IBiteWatcher biteWatcher;

        public MainWindow()
        {
            InitializeComponent();

            log4net.Config.XmlConfigurator.Configure(new FileStream("log4net.config", FileMode.Open));

            var l = (Logger)FishingBot.logger.Logger;
            l.AddAppender(this);

            DataContext = LogEntries = new ObservableCollection<LogEntry>();
            this.pixelClassifier = new PixelClassifier();
            this.bobberFinder = new SearchBobberFinder(pixelClassifier);

            var imageProvider = bobberFinder as IImageProvider;

            if (imageProvider != null)
            {
                imageProvider.BitmapEvent += ImageProvider_BitmapEvent;
            }

            int strikeValue = 7;
            this.biteWatcher = new PositionBiteWatcher(strikeValue);

            this.biteWatcher.BobberMoveEvent += BiteWatcher_BobberMoveEvent;
        }

        int lastAmplitude = 0;
        bool isLooting = false;

        private void BiteWatcher_BobberMoveEvent(object sender, BobberMoveEvent e)
        {
            if (e.Amplitude != lastAmplitude)
            {
                if (isLooting)
                {
                    isLooting = false;
                    this.Chart.ChartValues.Clear();
                }

                lastAmplitude = e.Amplitude;
                this.Chart.Add(e.Amplitude);

                if (e.Threshold)
                {
                    isLooting = true;
                    LootingGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    LootingGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            LogEntries.Insert(0, new LogEntry()
            {
                Index = index++,
                DateTime = DateTime.Now,
                Message = loggingEvent.RenderedMessage
            });

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        private FishingBot bot;

        private void SetImageVisibility(Image visible, Image collapsed, bool state)
        {
            visible.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            collapsed.Visibility = !state ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetImageVisibility()
        {
            SetImageVisibility(this.PlayImage, this.PlayImage_Disabled, this.Play.IsEnabled);
            SetImageVisibility(this.StopImage, this.StopImage_Disabled, this.Stop.IsEnabled);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var win2 = new ColourConfiguration(this.pixelClassifier);
            win2.Show();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (bot == null)
            {
                this.Play.IsEnabled = false;
                this.Stop.IsEnabled = true;
                SetImageVisibility();

                //var targetColor = System.Drawing.Color.FromArgb(-13168884);
                ConsoleKey castKey = (ConsoleKey)52;

                bot = new FishingBot(bobberFinder, this.biteWatcher, castKey);

                bot.Start();
                bot = null;
                this.Stop.IsEnabled = false;
                this.Play.IsEnabled = true;
                SetImageVisibility();
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            bot.Stop();
        }

        public static BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
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

        private void ImageProvider_BitmapEvent(object sender, BobberBitmapEvent e)
        {
            if (lastPoint.X == e.Point.X && lastPoint.Y == e.Point.Y)
            {
                System.Diagnostics.Debug.WriteLine("Ignore point");
                //return;
            }

            DrawOnBitmap(e.Bitmap, e.Point);

            BitmapImage bitmap = ToBitmapImage(e.Bitmap);
            var point = new Point(e.Point.X, e.Point.Y);
            e.Bitmap.Dispose();
            Application.Current.Dispatcher.BeginInvoke((Action)(() => this.Screenshot.Source = bitmap));

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        private void DrawOnBitmap(System.Drawing.Bitmap bmp, System.Drawing.Point bestp)
        {
            var p = bmp.GetPixel(bestp.X, bestp.Y);

            bmp.SetPixel(bestp.X, bestp.Y, System.Drawing.Color.White);

            //System.Diagnostics.Debug.WriteLine($"{p.R},{p.G},{p.B} at {bestp.X},{bestp.Y}");

            using (var gr = System.Drawing.Graphics.FromImage(bmp))
            {
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var thick_pen = new System.Drawing.Pen(System.Drawing.Color.White, 2))
                {
                    var cornerSize = 15;
                    var recSize = 40;
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(bestp.X - recSize, bestp.Y - recSize), cornerSize, cornerSize);
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(bestp.X - recSize, bestp.Y + recSize), cornerSize, -cornerSize);
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(bestp.X + recSize, bestp.Y - recSize), -cornerSize, cornerSize);
                    DrawCorner(thick_pen, gr, new System.Drawing.Point(bestp.X + recSize, bestp.Y + recSize), -cornerSize, -cornerSize);
                }
            }
        }

        private void DrawCorner(System.Drawing.Pen pen, System.Drawing.Graphics gr, System.Drawing.Point corner, int xDiff, int yDiff)
        {
            var lines = new System.Drawing.Point[]
            {
                new System.Drawing.Point(corner.X + xDiff, corner.Y),
                corner,
                new System.Drawing.Point(corner.X, corner.Y + yDiff)
            };

            gr.DrawLines(pen, lines);
        }
    }
}