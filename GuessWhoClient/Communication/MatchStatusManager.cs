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
        private List<IMatchStatusListener> subscribedPages = new List<IMatchStatusListener>();
        private string currentMatchCode;

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

        public void SubscribePage(IMatchStatusListener pageListener)
        {
            if (!subscribedPages.Contains(pageListener))
            {
                subscribedPages.Add(pageListener);
            }
        }

        public void UnsubscribePage(IMatchStatusListener pageListener)
        {
            subscribedPages.Remove(pageListener);
        }

        public void RestartRawValues()
        {
            client = null;
            subscribedPages = new List<IMatchStatusListener>();
        }

        public string CurrentMatchCode { get { return currentMatchCode; } set { currentMatchCode = value; } }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            foreach(var page in subscribedPages)
            {
                page.MatchStatusChanged(matchStatusCode);
            }
        }
    }
}
