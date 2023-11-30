using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using System;
using System.ServiceModel;
using System.Windows.Navigation;

namespace GuessWhoClient
{
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
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
