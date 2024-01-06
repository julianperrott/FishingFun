#nullable enable
namespace FishingFun
{
    using log4net.Appender;
    using log4net.Core;
    using log4net.Repository.Hierarchy;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;

    public partial class MainWindow : Window, IAppender
    {
        private System.Drawing.Point lastPoint = System.Drawing.Point.Empty;
        public ObservableCollection<LogEntry> LogEntries { get; set; }

        private IBobberFinder bobberFinder;
        private IPixelClassifier pixelClassifier;
        private IBiteWatcher biteWatcher;
        private ReticleDrawer reticleDrawer = new ReticleDrawer();

        private FishingBot? bot;
        private int strikeValue = 7; // this is the depth the bobber must go for the bite to be detected
        private bool setImageBackgroundColour = true;
        private Timer WindowSizeChangedTimer;
        private System.Threading.Thread? botThread;
        private int MacroTimer = 10;

        public MainWindow()
        {
            InitializeComponent();

            ((Logger)FishingBot.logger.Logger).AddAppender(this);

            this.DataContext = LogEntries = new ObservableCollection<LogEntry>();
            this.pixelClassifier = new PixelClassifier();
            pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());
             
            this.bobberFinder = new SearchBobberFinder(pixelClassifier);

            var imageProvider = bobberFinder as IImageProvider;
            if (imageProvider != null)
            {
                imageProvider.BitmapEvent += ImageProvider_BitmapEvent;
            }

            this.biteWatcher = new PositionBiteWatcher(strikeValue);

            this.WindowSizeChangedTimer = new Timer { AutoReset = false, Interval = 100 };
            this.WindowSizeChangedTimer.Elapsed += SizeChangedTimer_Elapsed;
            this.CardGrid.SizeChanged += MainWindow_SizeChanged;
            this.Closing += (s, e) => botThread?.Abort();

            this.KeyChooser.CastKeyChanged += (s, e) =>
            {
                this.Settings.Focus();
                this.bot?.SetCastKey(this.KeyChooser.CastKey);
            };

            this.Macro1KeyChooser.CastKeyChanged += (s, e) =>
            {
                this.Settings.Focus();
                this.bot?.SetMacro1Key(this.Macro1KeyChooser.CastKey);
            };

            this.Macro2KeyChooser.CastKeyChanged += (s, e) =>
            {
                this.Settings.Focus();
                this.bot?.SetMacro2Key(this.Macro2KeyChooser.CastKey);
            };
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Reset the timer so it only fires 100ms after the user stop dragging the window.
            WindowSizeChangedTimer.Stop();
            WindowSizeChangedTimer.Start();
        }

        private void SizeChangedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatch(() =>
            {
                this.flyingFishAnimation.AnimationWidth = (int)this.ActualWidth;
                this.flyingFishAnimation.AnimationHeight = (int)this.ActualHeight;
                this.LogGrid.Height = this.LogFlipper.ActualHeight;
                this.GraphGrid.Height = this.GraphFlipper.ActualHeight;
                this.GraphGrid.Visibility = Visibility.Visible;
                this.GraphFlipper.IsFlipped = true;
                this.LogFlipper.IsFlipped = true;
                this.GraphFlipper.IsFlipped = false;
                this.LogFlipper.IsFlipped = false;
            });
        }

        private void Stop_Click(object sender, RoutedEventArgs e) => bot?.Stop();

        private void Settings_Click(object sender, RoutedEventArgs e) => new ColourConfiguration(this.pixelClassifier).Show();

        private void CastKey_Click(object sender, RoutedEventArgs e) => this.KeyChooser.Focus();

        private void Macro1Key_Click(object sender, RoutedEventArgs e) => this.Macro1KeyChooser.Focus();

        private void Macro2Key_Click(object sender, RoutedEventArgs e) => this.Macro2KeyChooser.Focus();

        private void FishingEventHandler(object sender, FishingEvent e)
        {
            Dispatch(() =>
            {
                switch (e.Action)
                {
                    case FishingAction.BobberMove:
                        if (!this.GraphFlipper.IsFlipped)
                        {
                            this.Chart.Add(e.Amplitude);
                        }
                        break;

                    case FishingAction.Loot:
                        this.flyingFishAnimation.Start();
                        this.LootingGrid.Visibility = Visibility.Visible;
                        break;

                    case FishingAction.Cast:
                        this.Chart.ClearChart();
                        this.LootingGrid.Visibility = Visibility.Collapsed;
                        this.flyingFishAnimation.Stop();
                        setImageBackgroundColour = true;
                        break;
                };
            });
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            Dispatch(() =>
                LogEntries.Insert(0, new LogEntry()
                {
                    DateTime = DateTime.Now,
                    Message = loggingEvent.RenderedMessage
                })
            );
        }

        private void SetImageVisibility(Image imageForVisible, Image imageForCollapsed, bool state)
        {
            imageForVisible.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
            imageForCollapsed.Visibility = !state ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetButtonStates(bool isBotRunning)
        {
            Dispatch(() =>
            {
                this.Play.IsEnabled = isBotRunning;
                this.Stop.IsEnabled = !this.Play.IsEnabled;
                SetImageVisibility(this.PlayImage, this.PlayImage_Disabled, this.Play.IsEnabled);
                SetImageVisibility(this.StopImage, this.StopImage_Disabled, this.Stop.IsEnabled);
            });
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (bot == null)
            {
                WowProcess.PressKey(ConsoleKey.Spacebar);
                System.Threading.Thread.Sleep(1500);

                SetButtonStates(false);
                botThread = new System.Threading.Thread(new System.Threading.ThreadStart(this.BotThread));
                botThread.Start();

                // Hide cards after 10 minutes
                var timer = new Timer { Interval = 1000 * 60 * 10, AutoReset = false };
                timer.Elapsed += (s, ev) => this.Dispatch(() => this.LogFlipper.IsFlipped = this.GraphFlipper.IsFlipped = true);
                timer.Start();
            }
        }

        public void BotThread()
        {
            bot = new FishingBot(bobberFinder, this.biteWatcher, KeyChooser.CastKey, new List<ConsoleKey> { Macro1KeyChooser.CastKey, Macro2KeyChooser.CastKey }, MacroTimer);
            bot.FishingEventHandler += FishingEventHandler;
            bot.Start();

            bot = null;
            SetButtonStates(true);
        }

        private void ImageProvider_BitmapEvent(object sender, BobberBitmapEvent e)
        {
            Dispatch(() =>
            {
                SetBackgroundImageColour(e);
                reticleDrawer.Draw(e.Bitmap, e.Point);
                var bitmapImage = e.Bitmap.ToBitmapImage();
                e.Bitmap.Dispose();
                this.Screenshot.Source = bitmapImage;
            });
        }

        private void SetBackgroundImageColour(BobberBitmapEvent e)
        {
            if (this.setImageBackgroundColour)
            {
                this.setImageBackgroundColour = false;
                this.ImageBackground.Background = e.Bitmap.GetBackgroundColourBrush();
            }
        }

        private void Dispatch(Action action)
        {
            Application.Current?.Dispatcher.BeginInvoke((Action)(() => action()));
            Application.Current?.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate { }));
        }

        private void MacroTimer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(this.MacroTimerText.Text, out Int32 result))
            {
                this.MacroTimer = result;
                this.bot?.SetMacroTimer(this.MacroTimer);
            }
        }
    }
}