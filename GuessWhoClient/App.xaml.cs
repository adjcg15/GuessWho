using log4net;
using System.Windows;

namespace GuessWhoClient
{
    public partial class App : Application
    {
        public static readonly ILog log = LogManager.GetLogger(typeof(App));
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            base.OnStartup(e);
        }
    }
}
