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
            DataStore.UsersClient?.Unsubscribe();

            if(DataStore.Profile !=  null)
            {
                var authenticationServiceClient = new AuthenticationServiceClient();
                authenticationServiceClient.Logout(DataStore.Profile.NickName);
            }
        }
    }
}
