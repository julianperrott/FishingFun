using FishingFun;
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Powershell
{
    public class Program
    {
        private static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileStream("log4net.config", FileMode.Open));

            int strikeValue = 7;

            var pixelClassifier = new PixelClassifier();
            if (args.Contains("blue"))
            {
                Console.WriteLine("Blue mode");
                pixelClassifier.Mode = PixelClassifier.ClassifierMode.Blue;
            }

            pixelClassifier.SetConfiguration(WowProcess.IsWowClassic());

            var bobberFinder = new SearchBobberFinder(pixelClassifier);
            var biteWatcher = new PositionBiteWatcher(strikeValue);

            var bot = new FishingBot(bobberFinder, biteWatcher, ConsoleKey.D4, new List<ConsoleKey> { ConsoleKey.D5 }, 10);
            bot.FishingEventHandler += (b, e) => LogManager.GetLogger("Fishbot").Info(e);

            WowProcess.PressKey(ConsoleKey.Spacebar);
            System.Threading.Thread.Sleep(1500);

            bot.Start();
        }
    }
}