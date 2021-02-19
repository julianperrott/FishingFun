using FishingFun;
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using log4net.Repository;
using System.Reflection;

namespace Powershell
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());
            log4net.Config.XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

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

            var bot = new FishingBot(bobberFinder, biteWatcher, ConsoleKey.D4, new List<ConsoleKey> { ConsoleKey.D5 });
            bot.FishingEventHandler += (b, e) => LogManager.GetLogger(typeof(FishingBot)).Info(e);

            WowProcess.PressKey(ConsoleKey.Spacebar);
            System.Threading.Thread.Sleep(1500);

            bot.Start();
        }
    }
}