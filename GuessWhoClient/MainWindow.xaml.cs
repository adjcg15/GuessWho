using System.Windows.Navigation;

namespace GuessWhoClient
{
    public partial class MainWindow : NavigationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigationWindowClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DataStore.Profile !=  null)
            {
                GameServices.AuthenticationServiceClient authenticationServiceClient = new GameServices.AuthenticationServiceClient();
                authenticationServiceClient.Logout(DataStore.Profile.NickName);
            }
        }
    }
}
