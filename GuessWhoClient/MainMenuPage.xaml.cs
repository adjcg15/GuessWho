using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
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

        public void ShowHostCanceledMatchMessage()
        {
            UpdateUIMessage();
        }

        private void UpdateUIMessage()
        {
            BorderCanceledMatch.Visibility = Visibility.Visible;
            BorderOpacityCanceledMatch.Visibility = Visibility.Visible;
        }

        public void ShowCanceledMatchMessage()
        {
            UpdateUIMessage();

            LbMessageBoxMessage.Content = Properties.Resources.msgbCanceledMatchMessage;
            UpdateLanguageButton();
        }

        private void UpdateLanguageButton()
        {
            string currentLanguage = System.Threading.Thread.CurrentThread.CurrentUICulture.Name.ToUpper();

            if(currentLanguage == "ES-ES" || currentLanguage == "ES-MX")
            {
                TbBtnLanguage.Text = "ESPAÑOL";
                ImgBtnLanguage.Source = new BitmapImage(new Uri("/Resources/mx-flag-image.png", UriKind.Relative));
            }
            else
            {
                TbBtnLanguage.Text = "ENGLISH";
                ImgBtnLanguage.Source = new BitmapImage(new Uri("/Resources/us-flag-image.png", UriKind.Relative));

            }
        }

        private void BtnLoginClick(object sender, RoutedEventArgs e)
        {
            LoginPage loginPage = new LoginPage();
            NavigationService.Navigate(loginPage);
        }

        private void BtnRegisterClick(object sender, RoutedEventArgs e)
        {
            RegisterPage registerPage = new RegisterPage();
            NavigationService.Navigate(registerPage);
        }

        private void BtnLogOutClick(object sender, RoutedEventArgs e)
        {
            BtnLogOut.Visibility = Visibility.Collapsed;
            BtnTournamentMatch.Visibility = Visibility.Collapsed;
            BtnLogin.Visibility = Visibility.Visible;
            BtnRegister.Visibility = Visibility.Visible;
            BorderProfile.Visibility = Visibility.Collapsed;
            BtnFriends.Visibility = Visibility.Collapsed;

            try
            {
                AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
                authenticationServiceClient.Logout(DataStore.Profile.NickName);

                DataStore.Profile = null;
            }
            catch (EndpointNotFoundException ex)
            {
                App.log.Fatal(ex.Message);
                ServerResponse.ShowServerDownMessage();
            }
            catch(CommunicationException ex)
            {
                App.log.Error(ex.Message);
                ServerResponse.ShowConnectionLostMessage();
            }
        }

        private void BtnQuickMatchClick(object sender, RoutedEventArgs e)
        {
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            GameManager gameManager = GameManager.Instance;

            try
            {
                var createMatchResponse = gameManager.Client.CreateMatch(userNickname, false);

                gameManager.IsCurrentMatchHost = true;
                gameManager.CurrentMatchCode = createMatchResponse.Value;

                LobbyPage lobbyPage = new LobbyPage();
                NavigationService.Navigate(lobbyPage); 
            }
            catch (EndpointNotFoundException ex)
            {
                App.log.Fatal(ex.Message);

                gameManager.RestartRawValues();
                ServerResponse.ShowServerDownMessage();
            }
            catch (CommunicationException ex)
            {
                App.log.Error(ex.Message);

                gameManager.RestartRawValues();
                ServerResponse.ShowConnectionLostMessage();
            }
        }

        private void BtnLeaderboardClick(object sender, RoutedEventArgs e)
        {
            ScoreboardPage scoreboardPage = new ScoreboardPage();
            NavigationService.Navigate(scoreboardPage);
        }

        private void BtnJoinMatchClick(object sender, RoutedEventArgs e)
        {
            if(TbInvitationCode.Visibility == Visibility.Collapsed)
            {
                TbInvitationCode.Visibility = Visibility.Visible;
                ImgSendInvitationCode.Visibility = Visibility.Visible;
                TbCodePlaceholder.Visibility = Visibility.Visible;
                TbInvitationCode.Focus();
            }
            else
            {
                TbInvitationCode.Text = string.Empty;
                TbInvitationCode.Visibility = Visibility.Collapsed;
                ImgSendInvitationCode.Visibility = Visibility.Collapsed;
                TbCodePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnTournamentMatchClick(object sender, RoutedEventArgs e)
        {
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            GameManager gameManager = GameManager.Instance;

            try
            {
                var createMatchResponse = gameManager.Client.CreateMatch(userNickname, true);

                gameManager.IsCurrentMatchHost = true;
                gameManager.CurrentMatchCode = createMatchResponse.Value;

                LobbyPage lobbyPage = new LobbyPage();
                NavigationService.Navigate(lobbyPage);
            }
            catch (EndpointNotFoundException ex)
            {
                App.log.Fatal(ex.Message);

                gameManager.RestartRawValues();
                ServerResponse.ShowServerDownMessage();
            }
            catch (CommunicationException ex)
            {
                App.log.Error(ex.Message);

                gameManager.RestartRawValues();
                ServerResponse.ShowConnectionLostMessage();
            }
        }

        private void BtnFriendsClick(object sender, RoutedEventArgs e)
        {
            FriendsPage friendsPage = new FriendsPage();
            NavigationService.Navigate(friendsPage);
        }

        private void BorderProfileClick(object sender, RoutedEventArgs e)
        {
            ProfilePage profilePage = new ProfilePage();
            NavigationService.Navigate(profilePage);
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
                int cursorPosition = TbInvitationCode.CaretIndex;

                if (code.Length > INVITATION_CODE_LENGTH)
                {
                    code = previousCode; 
                    TbInvitationCode.Text = code;
                    TbInvitationCode.CaretIndex = cursorPosition;
                }

                code = new string(code.Where(char.IsLetterOrDigit).ToArray());

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

                previousCode = code;
            }
        }

        private void ImgSendInvitationCodeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GameManager gameManager = GameManager.Instance;
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            string invitationCode = TbInvitationCode.Text;

            try
            {
                var joinGameResponse = gameManager.Client.JoinGame(invitationCode, userNickname);
                switch (joinGameResponse.StatusCode)
                {
                    case ResponseStatus.OK:
                        gameManager.CurrentMatchCode = invitationCode;
                        gameManager.IsCurrentMatchHost = false;

                        PlayerInMatch host = joinGameResponse.Value;
                        if (host != null)
                        {
                            gameManager.AdversaryNickname = host.Nickname;
                            gameManager.AdversaryAvatar = host.Avatar;
                        }

                        LobbyPage lobby = new LobbyPage();
                        NavigationService.Navigate(lobby);
                        break;
                    case ResponseStatus.VALIDATION_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbInvalidMatchCodeMessage,
                            Properties.Resources.msgbInvalidMatchCodeTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        gameManager.RestartRawValues();
                        break;
                    case ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbHostLeftMatchMessage,
                            Properties.Resources.msgbHostLeftMatchTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        gameManager.RestartRawValues();
                        break;
                    case ResponseStatus.NOT_ALLOWED:
                        MessageBox.Show(
                            Properties.Resources.msgbErrorNotAllowedToJoinMatchMessage,
                            Properties.Resources.msgbErrorNotAllowedToJoinMatchTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        gameManager.RestartRawValues();
                        break;
                    default:
                        MessageBox.Show(
                            ServerResponse.GetMessageFromStatusCode(joinGameResponse.StatusCode),
                            Properties.Resources.msgbErrorJoiningMatchTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        gameManager.RestartRawValues();
                        break;
                }
            }
            catch (EndpointNotFoundException ex)
            {
                App.log.Fatal(ex.Message);

                gameManager.RestartRawValues();
                ServerResponse.ShowServerDownMessage();
            }
            catch (CommunicationException ex)
            {
                App.log.Error(ex.Message);

                gameManager.RestartRawValues();
                ServerResponse.ShowConnectionLostMessage();
            }
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

            UpdateLanguageButton();
        }
    }
}
