using log4net.Repository;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FishingFun
{
    public partial class App : Application
    {
        public App()
        {
            ILoggerRepository repository = log4net.LogManager.GetRepository(Assembly.GetCallingAssembly());
            log4net.Config.XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
        }
    }
}