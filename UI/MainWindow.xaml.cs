namespace FishingFun
{
    using log4net.Appender;
    using log4net.Core;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public partial class MainWindow : Window, IAppender
    {
        private System.Drawing.Point lastPoint = System.Drawing.Point.Empty;
        public ObservableCollection<LogEntry> LogEntries { get; set; }

        private SearchBobberFinder bobberFinder;
        private IPixelClassifier pixelClassifier;
        private IBiteWatcher biteWatcher;
        private ReticleDrawer reticleDrawer = new ReticleDrawer();

        private FishingBot bot;
        private int strikeValue = 7;

        public MainWindow()
        {
            this.Closing += MainWindow_Closing;

            InitializeComponent();
            this.DataContext = this;
            this.KeyChooser.FocusTarget = this.Play;

            ((Logger)FishingBot.logger.Logger).AddAppender(this);

            DataContext = LogEntries = new ObservableCollection<LogEntry>();
            this.pixelClassifier = new PixelClassifier();
            this.bobberFinder = new SearchBobberFinder(pixelClassifier);

            var imageProvider = bobberFinder as IImageProvider;
            if (imageProvider != null)
            {
                imageProvider.BitmapEvent += ImageProvider_BitmapEvent;
            }

            this.biteWatcher = new PositionBiteWatcher(strikeValue);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => bot?.Stop();
        private void Stop_Click(object sender, RoutedEventArgs e) => bot?.Stop();
        private void Settings_Click(object sender, RoutedEventArgs e) => new ColourConfiguration(this.pixelClassifier).Show();
        private void CastKey_Click(object sender, RoutedEventArgs e) => this.KeyChooser.Focus();

        private void FishingEventHandler(object sender, FishingEvent e)
        {
            switch (e.Action)
            {
                case FishingAction.BobberMove:
                    this.Chart.Add(e.Amplitude);
                    break;

                case FishingAction.Loot:
                    this.LootingGrid.Visibility = Visibility.Visible;
                    break;

                case FishingAction.Cast:
                    this.Chart.ClearChart();
                    this.LootingGrid.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            LogEntries.Insert(0, new LogEntry()
            {
                DateTime = DateTime.Now,
                Message = loggingEvent.RenderedMessage
            });
        }

        private void SetImageVisibility(Image imageForVisible, Image imageForCollapsed, bool state)
        {
            imageForVisible.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            imageForCollapsed.Visibility = !state ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetPlay(bool state)
        {
            this.Play.IsEnabled = state;
            this.Stop.IsEnabled = !state;

            SetImageVisibility(this.PlayImage, this.PlayImage_Disabled, this.Play.IsEnabled);
            SetImageVisibility(this.StopImage, this.StopImage_Disabled, this.Stop.IsEnabled);
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (bot == null)
            {
                SetPlay(true);

                bot = new FishingBot(bobberFinder, this.biteWatcher, KeyChooser.CastKey);
                bot.FishingEventHandler += FishingEventHandler;
                bot.Start();

                bot = null;
                SetPlay(false);
            }
        }

        private void ImageProvider_BitmapEvent(object sender, BobberBitmapEvent e)
        {
            reticleDrawer.Draw(e.Bitmap, e.Point);
            var bitmapImage = e.Bitmap.ToBitmapImage();
            e.Bitmap.Dispose();
            Dispatch(() => this.Screenshot.Source = bitmapImage);
        }

        private void Dispatch(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => action()));
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }
    }
}