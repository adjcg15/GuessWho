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
            if (!string.IsNullOrEmpty(DataStore.CurrentMatchCode))
            {
                if (DataStore.IsCurrentMatchHost)
                {
                    DataStore.MatchesClient.FinishGame(DataStore.CurrentMatchCode);
                }
                else
                {
                    DataStore.MatchesClient.ExitGame(DataStore.CurrentMatchCode);
                }
                DataStore.RestartMatchValues();
            }
        }

        private void UnsubscribeActiveUsersList()
        {
            DataStore.UsersClient?.Unsubscribe();
            DataStore.UsersClient = null;
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
