using GuessWhoClient.GameServices;
using System;
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
            if(!string.IsNullOrEmpty(DataStore.CurrentMatchCode))
            {
                if(DataStore.IsCurrentMatchHost)
                {
                    DataStore.MatchesClient.FinishGame(DataStore.CurrentMatchCode);
                }
                else
                {
                    DataStore.MatchesClient.ExitGame(DataStore.CurrentMatchCode);
                }
                DataStore.RestartMatchValues();
            }

            DataStore.UsersClient?.Unsubscribe();
            DataStore.UsersClient = null;

            if(DataStore.Profile !=  null)
            {
                var authenticationServiceClient = new AuthenticationServiceClient();
                authenticationServiceClient.Logout(DataStore.Profile.NickName);
            }
        }
    }
}
