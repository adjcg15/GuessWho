using GuessWhoClient.GameServices;
using System.Collections.Generic;
using System.ServiceModel;

namespace GuessWhoClient
{
    public class GameManager: IGameServiceCallback
    {
        private static GameManager instance;
        private GameServiceClient client;
        private List<IGamePage> subscribedPages = new List<IGamePage>();
        private string currentMatchCode;
        private bool isCurrentMatchHost;

        private GameManager() { }

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameManager();
                }
                return instance;
            }
        }

        public GameServiceClient Client 
        { 
            get { 
                if (client == null)
                {
                    client = new GameServiceClient(new InstanceContext(this));
                }

                return client; 
            } 
        }
        public string CurrentMatchCode { get { return currentMatchCode; } set { currentMatchCode = value; } }

        public bool IsCurrentMatchHost { get { return isCurrentMatchHost; } set { isCurrentMatchHost = value; } }

        public void SubscribePage(IGamePage page)
        {
            if (!subscribedPages.Contains(page))
            {
                subscribedPages.Add(page);
            }
        }

        public void UnsubscribePage(IGamePage page)
        {
            subscribedPages.Remove(page);
        }

        public void RestartRawValues()
        {
            isCurrentMatchHost = false;
            currentMatchCode = "";
            client = null;
            subscribedPages = new List<IGamePage>();
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            foreach (var page in subscribedPages)
            {
                page.PlayerStatusInMatchChanged(player, isInMatch);
            }
        }
    }
}
