using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class MainMenuPage : Page
    {
        private const int INVITATION_CODE_LENGTH = 8;
        private string previousCode = string.Empty;

        public MainMenuPage()
        {
            InitializeComponent();
        }

        public void ShowCanceledMatchMessage()
        {
            BorderCanceledMatch.Visibility = Visibility.Visible;
            BorderOpacityCanceledMatch.Visibility = Visibility.Visible;

            if(DataStore.Profile != null)
            {
                LoginProfile();
            }
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
            BtnFriends.Visibility = Visibility.Collapsed;

            AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();

            Console.WriteLine(DataStore.Profile?.NickName);
            authenticationServiceClient.Logout(DataStore.Profile.NickName);

            DataStore.Profile = null;
        }

        private void BtnQuickMatchClick(object sender, RoutedEventArgs e)
        {
            LobbyPage lobbyPage = new LobbyPage();
            this.NavigationService.Navigate(lobbyPage); 
        }

        private void BtnLeaderboardClick(object sender, RoutedEventArgs e)
        {
            ScoreboardPage scoreboardPage = new ScoreboardPage();
            this.NavigationService.Navigate(scoreboardPage);
        }

        private void BtnJoinMatchClick(object sender, RoutedEventArgs e)
        {
            if(TbInvitationCode.Visibility == Visibility.Collapsed)
            {
                TbInvitationCode.Visibility = Visibility.Visible;
                ImgSendInvitationCode.Visibility = Visibility.Visible;
                TbCodePlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                TbInvitationCode.Text = string.Empty;
                TbInvitationCode.Visibility = Visibility.Collapsed;
                ImgSendInvitationCode.Visibility = Visibility.Collapsed;
                TbCodePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnHowToPlayClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnTournamentMatchClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFriendsClick(object sender, RoutedEventArgs e)
        {
            FriendsPage friendsPage = new FriendsPage();
            this.NavigationService.Navigate(friendsPage);
        }

        private void BorderProfileClick(object sender, RoutedEventArgs e)
        {
            ProfilePage profilePage = new ProfilePage();
            this.NavigationService.Navigate(profilePage);
        }

        public void LoginProfile()
        {
            BtnLogOut.Visibility = Visibility.Visible;
            BtnTournamentMatch.Visibility = Visibility.Visible;
            BtnLogin.Visibility = Visibility.Collapsed;
            BtnRegister.Visibility = Visibility.Collapsed;
            BorderProfile.Visibility = Visibility.Visible;
            BtnFriends.Visibility = Visibility.Visible;

            lbNickname.Content = DataStore.Profile.NickName;

            BitmapImage profileImageSrc = ImageTransformator.GetBitmapImageFromByteArray(DataStore.Profile.Avatar);
            if (profileImageSrc != null)
            {
                ImgProfilePicture.Source = profileImageSrc;
            }
        }

        private void ReloadLanguageResources()
        {
            if(DataStore.Profile != null && TbBtnFriends.Visibility != Visibility.Collapsed)
            {
                TbBtnFriends.Text = Properties.Resources.btnFriends;
                TbBtnLogOut.Text = Properties.Resources.btnLogOut;
                TbBtnTournamentMatch.Text = Properties.Resources.btnTournamentMatch;
            }
            TbBtnLeaderboard.Text = Properties.Resources.btnLeaderboard;
            TbBtnJoinMatch.Text = Properties.Resources.btnJoinMatch;
            TbBtnLogin.Text = Properties.Resources.txtLoginGlobal;
            TbBtnQuickMatch.Text = Properties.Resources.btnQuickMatch;
            TbBtnRegister.Text = Properties.Resources.txtSignUpGlobal;

            TbCodePlaceholder.Text = Properties.Resources.tbCodePlaceholder;
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
        private void TbInvitationCodeTextChanged(object sender, TextChangedEventArgs e)
        {
            string code = TbInvitationCode.Text;

            if (code != previousCode)
            {
                previousCode = code;

                if (code.Length > INVITATION_CODE_LENGTH)
                {
                    TbInvitationCode.Text = code.Substring(0, INVITATION_CODE_LENGTH);
                    code = TbInvitationCode.Text;
                }

                if (!Regex.IsMatch(code, @"^[a-zA-Z0-9]*$"))
                {
                    TbInvitationCode.Text = new string(code.Where(char.IsLetterOrDigit).ToArray());
                    code = TbInvitationCode.Text;
                }

                if (code.Length == INVITATION_CODE_LENGTH)
                {
                    ImgSendInvitationCode.IsEnabled = true;
                    ImgSendInvitationCode.Opacity = 1.0;
                }
                else
                {
                    ImgSendInvitationCode.IsEnabled = false;
                    ImgSendInvitationCode.Opacity = 0.5;
                }
            }
        }

        private void ImgSendInvitationCodeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LobbyPage lobby = new LobbyPage(TbInvitationCode.Text);
            this.NavigationService.Navigate(lobby);
        }

        private void TbInvitationCodeGotFocus(object sender, RoutedEventArgs e)
        {
            TbInvitationCode.Focus();

            TbCodePlaceholder.Visibility = Visibility.Collapsed;
            TbInvitationCode.IsEnabled = true;
        }

        private void TbInvitationCodeLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TbInvitationCode.Text))
            {
                TbCodePlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void BorderCanceledMatchClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BorderCanceledMatch.Visibility = Visibility.Collapsed;
            BorderOpacityCanceledMatch.Visibility = Visibility.Collapsed;
        }

        private void MainMenuPageLoaded(object sender, RoutedEventArgs e)
        {
            if (DataStore.Profile != null)
            {
                LoginProfile();
            }
        }
    }
}
