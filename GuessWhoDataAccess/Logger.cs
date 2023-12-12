using log4net;
using System.IO;

namespace GuessWhoDataAccess
{
    public static class ServerLogger
    {
        public static ILog Instance { get; private set; }

        public static void ConfigureLogger()
        {
            Instance = LogManager.GetLogger(typeof(ServerLogger));
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
        }
    }
}
