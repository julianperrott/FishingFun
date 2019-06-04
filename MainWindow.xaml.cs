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
        private ReticleDrawer reticleDrawer = new ReticleDrawer();

        private ConsoleKey castKey = ConsoleKey.D4;
        private FishingBot bot;
        private int strikeValue = 7;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

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

            this.biteWatcher = new PositionBiteWatcher(strikeValue);
            this.biteWatcher.FishingEventHandler += FishingEventHandler;
        }

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
                case FishingAction.Reset:
                    this.Chart.ClearChart();
                    this.LootingGrid.Visibility = Visibility.Collapsed;
                    break;
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
        }

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

        private void CastKey_Click(object sender, RoutedEventArgs e)
        {
            KeyBind.Focus();
        }

        private void CastKey_Focus(object sender, RoutedEventArgs e)
        {
            KeyBind.Text = "";
        }

        private void KeyBind_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var key = e.Key.ToString();
            ProcessKeybindText(key);
        }

        private void ProcessKeybindText(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                ConsoleKey ck;
                if (Enum.TryParse<ConsoleKey>(key, out ck))
                {
                    this.castKey = ck;
                    this.Play.Focus();

                    KeyBind.Text = GetCastKeyText(this.castKey);
                    return;
                }
            }
            KeyBind.Text = "";
        }

        private void KeyBind_LostFocus(object sender, RoutedEventArgs e)
        {
            ProcessKeybindText(KeyBind.Text);
            if (string.IsNullOrEmpty(KeyBind.Text))
            {
                KeyBind.Text = GetCastKeyText(this.castKey);
            }
        }

        private string GetCastKeyText(ConsoleKey ck)
        {
            string keyText = ck.ToString();
            if (keyText.Length == 1) { return keyText; }
            if (keyText.StartsWith("D") && keyText.Length == 2)
            {
                return keyText.Substring(1, 1);
            }
            return "?";
        }
    }
}