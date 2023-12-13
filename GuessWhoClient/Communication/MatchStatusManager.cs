using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GuessWhoClient.Communication
{
    public class MatchStatusManager : IMatchStatusServiceCallback
    {
        private static MatchStatusManager instance;
        private MatchStatusServiceClient client;
        private List<IMatchStatusPage> subscribedPages = new List<IMatchStatusPage>();

        private MatchStatusManager() { }

        public static MatchStatusManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MatchStatusManager();
                }
                return instance;
            }
        }

        public MatchStatusServiceClient Client
        {
            get
            {
                if (client == null)
                {
                    client = new MatchStatusServiceClient(new InstanceContext(this));
                }

                return client;
            }
        }

        public void SubscribePage(IMatchStatusPage pageListener)
        {
            if (!subscribedPages.Contains(pageListener))
            {
                subscribedPages.Add(pageListener);
            }
        }

        public void UnsubscribePage(IMatchStatusPage pageListener)
        {
            subscribedPages.Remove(pageListener);
        }

        public void RestartRawValues()
        {
            client = null;
            subscribedPages = new List<IMatchStatusPage>();
        }

        public string CurrentMatchCode { get ; set ; }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            List<IMatchStatusPage> pagesCopy = new List<IMatchStatusPage>(subscribedPages);

            foreach (var page in pagesCopy)
            {
                page.MatchStatusChanged(matchStatusCode);
            }
        }
    }
}
