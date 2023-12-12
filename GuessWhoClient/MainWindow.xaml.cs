using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using log4net;
using System;
using System.ServiceModel;
using System.Windows.Navigation;

namespace GuessWhoClient
{
    public partial class MainWindow : NavigationWindow
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            InitializeComponent();
            log4net.Config.XmlConfigurator.Configure();
            log.Warn("Abriendo MainWindow");
        }

        private void NavigationWindowClosed(object sender, EventArgs e)
        {
            try
            {
                LeaveChatRoom();
                LeaveCurrentGame();
                UnsubscribeActiveUsersList();
                Logout();
            }
            catch (EndpointNotFoundException)
            {

            }
            catch (CommunicationObjectFaultedException)
            {

            }
            catch (CommunicationException)
            {

            }
        }

        private void LeaveCurrentGame()
        {
            GameManager gameManager = GameManager.Instance;
            MatchStatusManager matchStatusManager = MatchStatusManager.Instance;

            if (!string.IsNullOrEmpty(gameManager.CurrentMatchCode))
            {
                if (gameManager.IsCurrentMatchHost)
                {
                    gameManager.Client.FinishGame(gameManager.CurrentMatchCode);
                }
                else
                {
                    gameManager.Client.ExitGame(gameManager.CurrentMatchCode);
                }

                gameManager.RestartRawValues();
            }

            if(!string.IsNullOrEmpty(matchStatusManager.CurrentMatchCode))
            {
                matchStatusManager.Client.StopListeningMatchStatus(matchStatusManager.CurrentMatchCode);
                matchStatusManager.RestartRawValues();
            }
        }

        private void UnsubscribeActiveUsersList()
        {
            DataStore.UsersClient?.Unsubscribe();
            DataStore.UsersClient = null;
        }

        private void LeaveChatRoom()
        {
            GameManager gameManager = GameManager.Instance;

            if (gameManager.CurrentMatchCode != "")
            {
                DataStore.ChatsClient?.LeaveChatRoom(gameManager.CurrentMatchCode);
                DataStore.ChatsClient = null;
            }
        }

        private void Logout()
        {
            if (DataStore.Profile != null)
            {
                var authenticationServiceClient = new AuthenticationServiceClient();
                authenticationServiceClient.Logout(DataStore.Profile.NickName);
            }
        }
    }
}
