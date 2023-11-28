using GuessWhoClient.Communication;
using GuessWhoClient.Components;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using GuessWhoClient.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class LobbyPage : Page, IUserServiceCallback, IGamePage, IMatchStatusListener
    {
        private const string DEFAULT_PROFILE_PICTURE_ROUTE = "pack://application:,,,/Resources/user-icon.png";
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;
        public ObservableCollection<ActiveUser> activeUsers { get; set; } = new ObservableCollection<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();

            gameManager.IsCurrentMatchHost = true;
            gameManager.SubscribePage(this);
            matchStatusManager.SubscribePage(this);

            ShowCancelGameButton();
        }

        public LobbyPage(string invitationCode)
        {
            InitializeComponent();

            gameManager.IsCurrentMatchHost = false;
            gameManager.CurrentMatchCode = invitationCode;
            matchStatusManager.CurrentMatchCode = invitationCode;
            gameManager.SubscribePage(this);
            matchStatusManager.SubscribePage(this);

            ShowExitGameButton();
            HideInvitationOptions();
        }

        private void ShowExitGameButton()
        {
            BtnExitGame.Visibility = Visibility.Visible;
        }

        private void ShowCancelGameButton()
        {
            BtnCancelGame.Visibility = Visibility.Visible;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            DataStore.UsersClient = new UserServiceClient(new InstanceContext(this));

            try
            {
                SubscribeToActiveUsersList();
                if (gameManager.IsCurrentMatchHost)
                {
                    LoadActiveUsers();
                    CreateNewGame(userNickname);
                    ShowActiveUsersList();
                }
                else
                {
                    JoinGame(userNickname);
                    ShowGameFullMessage();
                    EnableSendMessageButton();
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                DataStore.UsersClient = null;
                gameManager.RestartRawValues();
                matchStatusManager.RestartRawValues();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void SubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Subscribe();
        }

        private void CreateNewGame(string userNickname)
        {
            var createMatchResponse = gameManager.Client.CreateMatch(userNickname);

            gameManager.CurrentMatchCode = createMatchResponse.Value;
            matchStatusManager.CurrentMatchCode = createMatchResponse.Value;

            matchStatusManager.Client.ListenMatchStatus(matchStatusManager.CurrentMatchCode);
        }

        private void ShowActiveUsersList()
        {
            if (activeUsers.Count > 0)
            {
                LbUsersListMessage.Visibility = Visibility.Hidden;
                ListBoxActiveUsers.Visibility = Visibility.Visible;
            }
            else
            {
                LbUsersListMessage.Content = Properties.Resources.lbNoPlayersOnline;
                LbUsersListMessage.Visibility = Visibility.Visible;

                ListBoxActiveUsers.Visibility = Visibility.Hidden;
            }
        }

        private void JoinGame(string userNickname)
        {
            var joinGameResponse = gameManager.Client.JoinGame(gameManager.CurrentMatchCode, userNickname);
            switch (joinGameResponse.StatusCode)
            {
                case ResponseStatus.OK:
                    PlayerInMatch host = joinGameResponse.Value;

                    if (host != null)
                    {
                        ShowUserInfoInBanner(host.Nickname, host.Avatar);
                        ShowUserInfoInChat(host.Nickname, host.Avatar);
                    }
                    else
                    {
                        MessageBox.Show(
                            Properties.Resources.msgbHostInformationMissingMessage,
                            Properties.Resources.msgbHostInformationMissingTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        ShowUserInfoInBanner(Properties.Resources.txtHost, null);
                        ShowUserInfoInChat(Properties.Resources.txtHost, null);
                    }
                    matchStatusManager.Client.ListenMatchStatus(matchStatusManager.CurrentMatchCode);
                    break;
                case ResponseStatus.VALIDATION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbInvalidMatchCodeMessage,
                        Properties.Resources.msgbInvalidMatchCodeTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    RedirectPermanentlyToMainMenu();
                    break;
                case ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbHostLeftMatchMessage,
                        Properties.Resources.msgbHostLeftMatchTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    RedirectPermanentlyToMainMenu();
                    break;
                default:
                    MessageBox.Show(
                        ServerResponse.GetMessageFromStatusCode(joinGameResponse.StatusCode),
                        Properties.Resources.msgbErrorJoiningMatchTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    RedirectPermanentlyToMainMenu();
                    break;
            }
        }

        private void ShowGameFullMessage()
        {
            string fullGameMessage;

            if(gameManager.IsCurrentMatchHost)
            {
                fullGameMessage = Properties.Resources.lbAllPlayersReady;
            }
            else
            {
                fullGameMessage = Properties.Resources.lbWaitGameToStart;
            }

            LbUsersListMessage.Content = fullGameMessage;
            LbUsersListMessage.Visibility = Visibility.Visible;

            ListBoxActiveUsers.Visibility = Visibility.Hidden;
        }

        private void RedirectPermanentlyToMainMenu()
        {
            DataStore.UsersClient = null;

            ShowsNavigationUI = true;
            MainMenuPage menuPage = new MainMenuPage();
            NavigationService.Navigate(menuPage);
        }

        private void LoadActiveUsers()
        {
            activeUsers = new ObservableCollection<ActiveUser>(DataStore.UsersClient.GetActiveUsers().ToList());
            if (DataStore.Profile != null)
            {
                activeUsers.Remove(activeUsers.FirstOrDefault(u => u.Nickname == DataStore.Profile.NickName));
            }

            ListBoxActiveUsers.ItemsSource = activeUsers;
        }

        public void UserStatusChanged(ActiveUser user, bool isActive)
        {
            if (isActive)
            {
                Application.Current.Dispatcher.Invoke(() => activeUsers.Add(user));
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var userToRemove = activeUsers.FirstOrDefault(u => u.Nickname == user.Nickname);
                    if (userToRemove != null)
                    {
                        activeUsers.Remove(userToRemove);
                    }
                });
            }
            ShowActiveUsersList();
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch user, bool isJoiningMatch)
        {
            bool isAdversaryCurrentMatchHost = !gameManager.IsCurrentMatchHost;

            if(isAdversaryCurrentMatchHost)
            {
                if(!isJoiningMatch)
                {
                    NotifyGameHasBeenCanceled();
                }
            }
            else
            {
                if (isJoiningMatch)
                {
                    ShowGuestInformation(user);
                    ShowGameFullMessage();
                    HideInvitationOptions();
                    ShowStartGameButton();
                    EnableSendMessageButton();
                }
                else
                {
                    HideGuestInformation();
                    ShowActiveUsersList();
                    ShowInvitationOptions();
                    HideStartGameButton();
                    DisableSendMessageButton();
                }
            }
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            if (matchStatusCode == MatchStatus.CharacterSelection)
            {
                Application.Current.Dispatcher.Invoke(() => RedirectToChooseCharacterPage());
            }
        }

        private void ShowStartGameButton()
        {
            BtnStartGame.Visibility = Visibility.Visible;
        }

        private void HideStartGameButton()
        {
            BtnStartGame.Visibility = Visibility.Hidden;
        }

        private void NotifyGameHasBeenCanceled()
        {
            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
            MainMenuPage mainMenu = new MainMenuPage();
            mainMenu.ShowCanceledMatchMessage();
            this.NavigationService.Navigate(mainMenu);
        }

        private void ShowGuestInformation(PlayerInMatch guest)
        {
            ShowUserInfoInBanner(guest.Nickname, guest.Avatar);
            ShowUserInfoInChat(guest.Nickname, guest.Avatar);
        }

        private void ShowUserInfoInBanner(string nickname, byte[] avatar)
        {
            BorderOponent.Background = new SolidColorBrush(Color.FromRgb(182, 216, 242));
            TbOponent.Text = nickname != "" ? nickname : Properties.Resources.txtGuest;
            if (avatar != null)
            {
                ImgProfilePicture.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(avatar);
            }
        }

        private void ShowUserInfoInChat(string nickname, byte[] avatar)
        {
            TbOponentChat.Text = nickname != "" ? nickname : Properties.Resources.txtGuest;
            if (avatar != null)
            {
                ImgChatProfilePicture.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(avatar);
            }
        }

        private void HideGuestInformation()
        {
            ShowDefaultUserInfoInBanner();
            ShowDefaultUserInfoInChat();
        }

        private void ShowDefaultUserInfoInBanner()
        {
            BorderOponent.Background = new SolidColorBrush(Color.FromRgb(226, 226, 226));
            TbOponent.Text = Properties.Resources.lbWaitingPlayer;
            Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
            BitmapImage defaultImage = new BitmapImage(uri);
            ImgProfilePicture.ImageSource = defaultImage;
        }

        private void ShowDefaultUserInfoInChat()
        {
            TbOponentChat.Text = Properties.Resources.lbWaitingPlayer;
            Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
            BitmapImage defaultImage = new BitmapImage(uri);
            ImgChatProfilePicture.ImageSource = defaultImage;
        }

        private void ShowInvitationOptions()
        {
            BtnInvitationCode.Visibility = Visibility.Visible;
            LbInvitePlayer.Visibility = Visibility.Visible;
        }

        private void HideInvitationOptions()
        {
            BtnInvitationCode.Visibility = Visibility.Hidden;
            LbInvitePlayer.Visibility = Visibility.Hidden;
        }

        private void EnableSendMessageButton()
        {
            BtnSendMessage.Opacity = 1;
            BtnSendMessage.IsEnabled = true;
        }

        private void DisableSendMessageButton()
        {
            BtnSendMessage.Opacity = 0.5;
            BtnSendMessage.IsEnabled = false;
        }

        private void BtnSendMessageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMessage();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void SendMessage()
        {
            string message = TbMessage.Text;

            if (!string.IsNullOrEmpty(message))
            {
                
            }
        }

        private void ShowOwnMessageInChat(string message)
        {
            OwnChatMessage messageElement = new OwnChatMessage();
            messageElement.TbMessage.Text = message;
            SpChatMessages.Children.Add(messageElement);
            SvChatMessages.ScrollToBottom();
            TbMessage.Text = "";
        }

        public void NotifyNewMessage(string message, string senderNickname)
        {
            string defaultAdversaryNickname = gameManager.IsCurrentMatchHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
            ShowPlayerMessageInChat(message, senderNickname == "" ? defaultAdversaryNickname : senderNickname);
        }

        private void ShowPlayerMessageInChat(string message, string playerNickname)
        {
            ChatMessage messageElement = new ChatMessage();
            messageElement.TbMessage.Text = message;
            messageElement.TbNickname.Text = playerNickname;
            SpChatMessages.Children.Add(messageElement);
            SvChatMessages.ScrollToBottom();
            TbMessage.Text = "";
        }

        private void BtnCopyInvitationCodeClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(gameManager.CurrentMatchCode);
        }

        private void BtnExitGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UnsubscribeToActiveUsersList();
                ExitGame();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void UnsubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Unsubscribe();
            DataStore.UsersClient = null;
        }

        private void ExitGame()
        {
            gameManager.Client.ExitGame(gameManager.CurrentMatchCode);
            gameManager.RestartRawValues();
            matchStatusManager.Client.StopListeningMatchStatus(matchStatusManager.CurrentMatchCode);
            matchStatusManager.RestartRawValues();
            RedirectPermanentlyToMainMenu();
        }

        private void BtnFinishGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UnsubscribeToActiveUsersList();
                FinishGame();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void FinishGame()
        {
            gameManager.Client.FinishGame(gameManager.CurrentMatchCode);
            gameManager.RestartRawValues();
            matchStatusManager.Client.StopListeningMatchStatus(matchStatusManager.CurrentMatchCode);
            matchStatusManager.RestartRawValues();
            RedirectPermanentlyToMainMenu();
        }

        private void BtnStartGameClick(object sender, RoutedEventArgs e)
        {
            matchStatusManager.Client.StartCharacterSelection(matchStatusManager.CurrentMatchCode);

            Application.Current.Dispatcher.Invoke(() => RedirectToChooseCharacterPage());
        }

        private void RedirectToChooseCharacterPage()
        {
            ChooseCharacterPage characterPage = new ChooseCharacterPage();
            NavigationService.Navigate(characterPage);
        }

        private void BtnInviteToGameClick(object sender, RoutedEventArgs e)
        {
            Button invitationButton = e.Source as Button;
            ActiveUser activeUser = (ActiveUser)invitationButton.DataContext;

            string nickname = activeUser.Nickname;
            try
            {
                bool playerInvitedSuccessfully = SendInvitationToUser(nickname);

                if(playerInvitedSuccessfully)
                {
                    MessageBox.Show(
                        Properties.Resources.msgbInvitationSentMessage,
                        Properties.Resources.msgbInvitationSentTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                    invitationButton.Visibility = Visibility.Hidden;
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }

        private bool SendInvitationToUser(string nickname)
        {
            bool successSent = false;
            var authenticationClient = new AuthenticationServiceClient();
            ProfileResponse response = authenticationClient.VerifyUserRegisteredByNickName(nickname);

            switch(response.StatusCode)
            {
                case ResponseStatus.OK:
                    if(response.Value != null)
                    {
                        successSent = SendEmailInvitation(response.Value.Email);
                    }
                    break;
                case ResponseStatus.SQL_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbErrorRetrievingPlayerEmailMessage,
                        Properties.Resources.msgbErrorRetrievingPlayerEmailTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
                default:
                    MessageBox.Show(
                        ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                        Properties.Resources.msgbErrorSendingGameInvitationTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
            }

            return successSent;
        }

        private bool SendEmailInvitation(string email)
        {
            bool successSent = Email.SendMail(
                email,
                Properties.Resources.txtInviteToGameSubject,
                Properties.Resources.txtInviteToGameBody + gameManager.CurrentMatchCode
            );

            if (!successSent)
            {
                MessageBox.Show(
                    Properties.Resources.msgbErrorSendingGameInvitationMessage,
                    Properties.Resources.msgbErrorSendingGameInvitationTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            return successSent;
        }
    }
}