using FishingFun;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System;
using System.IO;

namespace FishingFunConsole
{
    public class Program
    {
        private static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileStream("log4net.config", FileMode.Open));
           
            int strikeValue = 7;

            var pixelClassifier = new PixelClassifier();
            var bobberFinder = new SearchBobberFinder(pixelClassifier);
            var biteWatcher = new PositionBiteWatcher(strikeValue);

            var bot = new FishingBot(bobberFinder, biteWatcher, ConsoleKey.D5);
            bot.FishingEventHandler += (b, e) => LogManager.GetLogger("Fishbot").Info(e);
            bot.Start();
        }
    }
}