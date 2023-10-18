using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            this.NavigationService.Navigate(loginPage);
        }

        private void BtnRegisterClick(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            this.NavigationService.Navigate(registerPage);
        }

        private void BtnLogOutClick(object sender, RoutedEventArgs e)
        {
            BtnLogOut.Visibility = Visibility.Collapsed;
            BtnTournamentMatch.Visibility = Visibility.Collapsed;
            BtnLogin.Visibility = Visibility.Visible;
            BtnRegister.Visibility = Visibility.Visible;
            BorderProfile.Visibility = Visibility.Collapsed;
            BtnLeaderboard.Visibility = Visibility.Collapsed;
            BtnFriends.Visibility = Visibility.Collapsed;

            AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();

            Console.WriteLine(ProfileSingleton.Instance);
            Console.WriteLine(ProfileSingleton.Instance?.NickName);
            authenticationServiceClient.Logout(ProfileSingleton.Instance.NickName);

            ProfileSingleton.Instance = null;
        }

        private void BtnQuickMatchClick(object sender, RoutedEventArgs e)
        {
            LobbyPage lobbyPage = new LobbyPage();
            this.NavigationService.Navigate(lobbyPage); 
        }

        private void BtnLeaderboardClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnJoinMatchClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnHowToPlayClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTournamentMatchClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFriendsClick(object sender, RoutedEventArgs e)
        {

        }

        public void LoginProfile()
        {
            BtnLogOut.Visibility = Visibility.Visible;
            BtnTournamentMatch.Visibility = Visibility.Visible;
            BtnLogin.Visibility = Visibility.Collapsed;
            BtnRegister.Visibility = Visibility.Collapsed;
            BorderProfile.Visibility = Visibility.Visible;
            BtnLeaderboard.Visibility = Visibility.Visible;
            BtnFriends.Visibility = Visibility.Visible;

            lbNickname.Content = ProfileSingleton.Instance.NickName;

            BitmapImage profileImageSrc = ImageTransformator.GetBitmapImageFromByteArray(ProfileSingleton.Instance.Avatar);
            if (profileImageSrc != null)
            {
                ImgProfilePicture.Source = profileImageSrc;
            }
        }

        private void ReloadLanguageResources()
        {
            if(ProfileSingleton.Instance != null && TbBtnFriends.Visibility != Visibility.Collapsed)
            {
                TbBtnFriends.Text = Properties.Resources.btnFriends;
                TbBtnLeaderboard.Text = Properties.Resources.btnLeaderboard;
                TbBtnLogOut.Text = Properties.Resources.btnLogOut;
                TbBtnTournamentMatch.Text = Properties.Resources.btnTournamentMatch;
            }
            TbBtnJoinMatch.Text = Properties.Resources.btnJoinMatch;
            TbBtnLogin.Text = Properties.Resources.txtLoginGlobal;
            TbBtnQuickMatch.Text = Properties.Resources.btnQuickMatch;
            TbBtnRegister.Text = Properties.Resources.txtSignUpGlobal;
        }

        private void BtnChangeLanguageClick(object sender, RoutedEventArgs e)
        {
            if (TbBtnLanguage.Text == "ESPAÑOL")
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                TbBtnLanguage.Text = "ENGLISH";
                ImgBtnLanguage.Source = new BitmapImage(new Uri("/Resources/us-flag-image.png", UriKind.Relative));
            }
            else if (TbBtnLanguage.Text == "ENGLISH")
            {
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
                TbBtnLanguage.Text = "ESPAÑOL";
                ImgBtnLanguage.Source = new BitmapImage(new Uri("/Resources/mx-flag-image.png", UriKind.Relative));
            }

            ReloadLanguageResources();
        }

    }
}
