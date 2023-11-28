using GuessWhoClient.Components;
using GuessWhoClient.GameServices;
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
    public partial class LobbyPage : Page, IUserServiceCallback, IChatServiceCallback, IGamePage
    {
        private const string DEFAULT_PROFILE_PICTURE_ROUTE = "pack://application:,,,/Resources/user-icon.png";
        private readonly GameManager gameManager = GameManager.Instance;
        public ObservableCollection<ActiveUser> activeUsers { get; set; } = new ObservableCollection<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();

            gameManager.IsCurrentMatchHost = true;
            gameManager.SubscribePage(this);

            ShowCancelGameButton();
        }

        public LobbyPage(string invitationCode)
        {
            InitializeComponent();

            gameManager.IsCurrentMatchHost = false;
            gameManager.CurrentMatchCode = invitationCode;
            gameManager.SubscribePage(this);

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
            DataStore.ChatsClient = new ChatServiceClient(new InstanceContext(this));

            try
            {
                SubscribeToActiveUsersList();
                if (gameManager.IsCurrentMatchHost)
                {
                    LoadActiveUsers();
                    CreateNewGame(userNickname);
                    EnterToChatRoom();
                    ShowActiveUsersList();
                }
                else
                {
                    JoinGame(userNickname);
                    EnterToChatRoom();
                    ShowGameFullMessage();
                    EnableSendMessageButton();
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void ClearCommunicationChannels()
        {
            DataStore.UsersClient = null;
            DataStore.ChatsClient = null;
            gameManager.RestartRawValues();
        }

        private void SubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Subscribe();
        }

        private void CreateNewGame(string userNickname)
        {
            var createMatchResponse = gameManager.Client.CreateMatch(userNickname);
            gameManager.CurrentMatchCode = createMatchResponse.Value;
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
                    gameManager.AdversaryNickname = host.Nickname;
                    gameManager.AdversaryAvatar = host.Avatar;

                    if (host != null)
                    {
                        ShowAdversaryInfoInBanner();
                        ShowAdversaryInfoInChat();
                    }
                    else
                    {
                        MessageBox.Show(
                            Properties.Resources.msgbHostInformationMissingMessage,
                            Properties.Resources.msgbHostInformationMissingTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );

                        gameManager.AdversaryNickname = Properties.Resources.txtHost;
                        gameManager.AdversaryAvatar = null;
                        ShowAdversaryInformation();
                    }
                    break;
                case ResponseStatus.VALIDATION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbInvalidMatchCodeMessage,
                        Properties.Resources.msgbInvalidMatchCodeTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    ClearCommunicationChannels();
                    RedirectPermanentlyToMainMenu();
                    break;
                case ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbHostLeftMatchMessage,
                        Properties.Resources.msgbHostLeftMatchTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    ClearCommunicationChannels();
                    RedirectPermanentlyToMainMenu();
                    break;
                default:
                    MessageBox.Show(
                        ServerResponse.GetMessageFromStatusCode(joinGameResponse.StatusCode),
                        Properties.Resources.msgbErrorJoiningMatchTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    ClearCommunicationChannels();
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

        public void EnterToChatRoom()
        {
            DataStore.ChatsClient.EnterToChatRoom(gameManager.CurrentMatchCode);
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch user, bool isJoiningMatch)
        {
            bool isAdversaryCurrentMatchHost = !gameManager.IsCurrentMatchHost;
            gameManager.AdversaryNickname = user.Nickname;
            gameManager.AdversaryAvatar = user.Avatar;

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
                    ShowAdversaryInformation();
                    ShowGameFullMessage();
                    HideInvitationOptions();
                    ShowStartGameButton();
                    EnableSendMessageButton();
                }
                else
                {
                    HideAdversaryInformation();
                    ShowActiveUsersList();
                    ShowInvitationOptions();
                    HideStartGameButton();
                    DisableSendMessageButton();
                }
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
            MainMenuPage mainMenu = new MainMenuPage();
            mainMenu.ShowCanceledMatchMessage();
            NavigationService.Navigate(mainMenu);
        }

        private void ShowAdversaryInformation()
        {
            ShowAdversaryInfoInBanner();
            ShowAdversaryInfoInChat();
        }

        private void ShowAdversaryInfoInBanner()
        {
            BorderOponent.Background = new SolidColorBrush(Color.FromRgb(182, 216, 242));

            string defaultAdversaryNickname = gameManager.IsCurrentMatchHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
            TbOponent.Text = !string.IsNullOrEmpty(gameManager.AdversaryNickname) ? gameManager.AdversaryNickname : defaultAdversaryNickname;
            if (gameManager.AdversaryAvatar != null)
            {
                ImgProfilePicture.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(gameManager.AdversaryAvatar);
            }
        }

        private void ShowAdversaryInfoInChat()
        {
            string defaultAdversaryNickname = gameManager.IsCurrentMatchHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
            TbOponentChat.Text = !string.IsNullOrEmpty(gameManager.AdversaryNickname) ? gameManager.AdversaryNickname : defaultAdversaryNickname;
            if (gameManager.AdversaryAvatar != null)
            {
                ImgChatProfilePicture.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(gameManager.AdversaryAvatar);
            }
        }

        private void HideAdversaryInformation()
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
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void SendMessage()
        {
            string message = TbMessage.Text;

            if (!string.IsNullOrEmpty(message))
            {
                var response = DataStore.ChatsClient.SendMessage(gameManager.CurrentMatchCode, message);
                switch (response.StatusCode)
                {
                    case ResponseStatus.OK:
                        if (response.Value)
                        {
                            ShowOwnMessageInChat(message);
                        }
                        break;
                    case ResponseStatus.VALIDATION_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbSendMessageMatchFinishedMessage,
                            Properties.Resources.msgbInvalidMatchCodeTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        ClearCommunicationChannels();
                        RedirectPermanentlyToMainMenu();
                        break;
                    case ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbMesageNotSentMessage,
                            Properties.Resources.msgbMessageNotSentTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        if (!gameManager.IsCurrentMatchHost)
                        {
                            ClearCommunicationChannels();
                            RedirectPermanentlyToMainMenu();
                        }
                        else
                        {
                            DisableSendMessageButton();
                            ShowDefaultUserInfoInChat();
                            ShowDefaultUserInfoInBanner();
                        }
                        break;
                    default:
                        MessageBox.Show(
                            ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                            Properties.Resources.msgbInvalidMatchCodeTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        ClearCommunicationChannels();
                        RedirectPermanentlyToMainMenu();
                        break;
                }
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

        public void NewMessageReceived(string message)
        {
            string defaultAdversaryNickname = gameManager.IsCurrentMatchHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
            ShowAdversaryMessageInChat(message, string.IsNullOrEmpty(gameManager.AdversaryNickname) ? defaultAdversaryNickname : gameManager.AdversaryNickname);
        }

        private void ShowAdversaryMessageInChat(string message, string adversaryNickname)
        {
            ChatMessage messageElement = new ChatMessage();
            messageElement.TbMessage.Text = message;
            messageElement.TbNickname.Text = adversaryNickname;
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
                ExitChatRoom();
                ExitGame();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void UnsubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Unsubscribe();
            DataStore.UsersClient = null;
        }

        public void ExitChatRoom()
        {
            DataStore.ChatsClient.LeaveChatRoom(gameManager.CurrentMatchCode);
            DataStore.ChatsClient = null;
        }

        private void ExitGame()
        {
            gameManager.Client.ExitGame(gameManager.CurrentMatchCode);
            gameManager.RestartRawValues();
            RedirectPermanentlyToMainMenu();
        }

        private void BtnFinishGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UnsubscribeToActiveUsersList();
                ExitChatRoom();
                FinishGame();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void FinishGame()
        {
            gameManager.Client.FinishGame(gameManager.CurrentMatchCode);
            gameManager.RestartRawValues();
            RedirectPermanentlyToMainMenu();
        }

        private void BtnStartGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UnsubscribeToActiveUsersList();
                ExitChatRoom();
                NavigateToChooseCharacterPage();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void NavigateToChooseCharacterPage()
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