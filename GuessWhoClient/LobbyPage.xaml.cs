using GuessWhoClient.Communication;
using GuessWhoClient.Components;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GuessWhoClient
{
    public partial class LobbyPage : Page, IUserServiceCallback, IGamePage, IMatchStatusPage, IChatServiceCallback
    {
        private const string DEFAULT_PROFILE_PICTURE_ROUTE = "pack://application:,,,/Resources/user-icon.png";
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;
        public ObservableCollection<UserCard> activeUsers { get; set; } = new ObservableCollection<UserCard>();

        public LobbyPage()
        {
            InitializeComponent();

            gameManager.SubscribePage(this);
            matchStatusManager.SubscribePage(this);

            if(gameManager.IsCurrentMatchHost)
            {
                ShowCancelGameButton();
            }
            else
            {
                ShowExitGameButton();
                HideInvitationOptions();
            }
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
            DataStore.UsersClient = new UserServiceClient(new InstanceContext(this));
            DataStore.ChatsClient = new ChatServiceClient(new InstanceContext(this));

            try
            {
                SubscribeToActiveUsersList();
                if (gameManager.IsCurrentMatchHost)
                {
                    StartListeningMatchStatus();
                    EnterToChatRoom();
                    LoadActiveUsers();
                    ShowActiveUsersList();
                }
                else
                {
                    ShowHostInformation();
                    StartListeningMatchStatus();
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

        public void EnterToChatRoom()
        {
            DataStore.ChatsClient.EnterToChatRoom(gameManager.CurrentMatchCode);
        }

        private void SubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Subscribe();
        }

        private void RedirectPermanentlyToMainMenu()
        {
            ShowsNavigationUI = true;
            MainMenuPage menuPage = new MainMenuPage();
            NavigationService.Navigate(menuPage);
        }

        private void ClearCommunicationChannels()
        {
            DataStore.UsersClient = null;
            DataStore.ChatsClient = null;
            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
        }

        private void LoadActiveUsers()
        {
            List<string> activeUsersNicknames = DataStore.UsersClient.GetActiveUsers().ToList();

            Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
            BitmapImage defaultImage = new BitmapImage(uri);

            List<UserCard> usersWithFormat = new List<UserCard>();
            foreach (string nickname in activeUsersNicknames)
            {
                usersWithFormat.Add(new UserCard
                {
                    Avatar = defaultImage,
                    Nickname = nickname
                });
            }

            activeUsers = new ObservableCollection<UserCard>(usersWithFormat);
            if (DataStore.Profile != null)
            {
                activeUsers.Remove(activeUsers.FirstOrDefault((user) => user.Nickname == DataStore.Profile.NickName));
            }

            ListBoxActiveUsers.ItemsSource = activeUsers;
            LoadActiveUsersAvatars();
        }

        private void LoadActiveUsersAvatars()
        {
            foreach(UserCard activeUser in activeUsers)
            {
                Task.Run(() => LoadUserAvatar(activeUser));
            }
        }

        private async void LoadUserAvatar(UserCard activeUser)
        {
            if (activeUser != null)
            {
                try
                {
                    var loadResponse = await ImageTransformator.LoadUserAvatarLazily(activeUser.Nickname);
                    if (loadResponse.Value != null)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            activeUser.Avatar = ImageTransformator.GetBitmapImageFromByteArray(loadResponse.Value);
                        });
                    }
                }
                catch (EndpointNotFoundException)
                {
                    ServerResponse.ShowServerDownMessage();
                    ClearCommunicationChannels();
                    RedirectPermanentlyToMainMenu();
                }
            }
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

        private void ShowGameFullMessage()
        {
            string fullGameMessage;

            if (gameManager.IsCurrentMatchHost)
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

        private void StartListeningMatchStatus()
        {
            matchStatusManager.CurrentMatchCode = gameManager.CurrentMatchCode;
            matchStatusManager.Client.ListenMatchStatus(matchStatusManager.CurrentMatchCode, DataStore.Profile?.NickName);
        }

        private void ShowHostInformation()
        {
            if (!string.IsNullOrEmpty(gameManager.AdversaryNickname))
            {
                ShowAdversaryInfoInBanner();
            }
            else
            {
                gameManager.AdversaryNickname = Properties.Resources.txtHost;
                gameManager.AdversaryAvatar = null;
            }

            ShowAdversaryInformation();
        }

        public void UserStatusChanged(string userNickname, bool isActive)
        {
            Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
            BitmapImage defaultImage = new BitmapImage(uri);

            if (isActive)
            {
                UserCard newUser = new UserCard
                {
                    Nickname = userNickname,
                    Avatar = defaultImage
                };
                activeUsers.Add(newUser);
                Task.Run(() => LoadUserAvatar(newUser));
            }
            else
            {
                var userToRemove = activeUsers.FirstOrDefault(user => user.Nickname == userNickname);
                if (userToRemove != null)
                {
                    activeUsers.Remove(userToRemove);
                }
            }
            ShowActiveUsersList();
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
                    try
                    {
                        UnsubscribeToActiveUsersList();
                        ExitChatRoom();
                        StopListeningMatchStatus();
                    }
                    catch(EndpointNotFoundException)
                    {
                        //TO-DO: log exception
                    }

                    ClearCommunicationChannels();
                    RedirectToMainMenuGameCanceled();
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

        public void StopListeningMatchStatus()
        {
            matchStatusManager.Client.StopListeningMatchStatus(matchStatusManager.CurrentMatchCode);
            matchStatusManager.RestartRawValues();
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            try
            {
                UnsubscribeToActiveUsersList();
                ExitChatRoom();

                if (matchStatusCode == MatchStatus.CharacterSelection)
                {
                    NavigateToChooseCharacterPage();
                }
            }
            catch(EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void NavigateToChooseCharacterPage()
        {
            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);

            ChooseCharacterPage characterPage = new ChooseCharacterPage();
            gameManager.SubscribePage(characterPage);
            matchStatusManager.SubscribePage(characterPage);

            NavigationService.Navigate(characterPage);
        }

        private void ShowStartGameButton()
        {
            BtnStartGame.Visibility = Visibility.Visible;
        }

        private void HideStartGameButton()
        {
            BtnStartGame.Visibility = Visibility.Hidden;
        }

        private void RedirectToMainMenuGameCanceled()
        {
            MainMenuPage mainMenu = new MainMenuPage();
            mainMenu.ShowHostCanceledMatchMessage();
            NavigationService.Navigate(mainMenu);
        }

        private void ShowAdversaryInformation()
        {
            ShowAdversaryInfoInBanner();
            ShowAdversaryInfoInChat();
        }

        private void ShowAdversaryInfoInBanner()
        {
            BorderAdversaryBanner.Background = new SolidColorBrush(Color.FromRgb(182, 216, 242));

            string defaultAdversaryNickname = gameManager.IsCurrentMatchHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
            string adversaryNickname = !string.IsNullOrEmpty(gameManager.AdversaryNickname) ? gameManager.AdversaryNickname : defaultAdversaryNickname;
            TbAdversaryNicknameInBanner.Text = adversaryNickname;
            gameManager.AdversaryNickname = adversaryNickname;

            if (gameManager.AdversaryAvatar != null)
            {
                ImgAdversaryAvatarInBanner.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(gameManager.AdversaryAvatar);
            }
        }

        private void ShowAdversaryInfoInChat()
        {
            string defaultAdversaryNickname = gameManager.IsCurrentMatchHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
            string adversaryNickname = !string.IsNullOrEmpty(gameManager.AdversaryNickname) ? gameManager.AdversaryNickname : defaultAdversaryNickname;
            TbAdversaryNicknameInChat.Text = adversaryNickname;

            if (gameManager.AdversaryAvatar != null)
            {
                ImgAdversaryAvatarInChat.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(gameManager.AdversaryAvatar);
            }
        }

        private void HideAdversaryInformation()
        {
            ShowDefaultUserInfoInBanner();
            ShowDefaultUserInfoInChat();
        }

        private void ShowDefaultUserInfoInBanner()
        {
            BorderAdversaryBanner.Background = new SolidColorBrush(Color.FromRgb(226, 226, 226));
            TbAdversaryNicknameInBanner.Text = Properties.Resources.lbWaitingOpponent;

            Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
            BitmapImage defaultImage = new BitmapImage(uri);
            ImgAdversaryAvatarInBanner.ImageSource = defaultImage;
        }

        private void ShowDefaultUserInfoInChat()
        {
            TbAdversaryNicknameInChat.Text = Properties.Resources.lbWaitingOpponent;
            Uri uri = new Uri(DEFAULT_PROFILE_PICTURE_ROUTE);
            BitmapImage defaultImage = new BitmapImage(uri);
            ImgAdversaryAvatarInChat.ImageSource = defaultImage;
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
                if(response.StatusCode == ResponseStatus.OK)
                {
                    if (response.Value)
                    {
                        ShowOwnMessageInChat(message);
                    }
                }
                else
                {
                    ShowErrorSendingMessage(response.StatusCode);

                    if (gameManager.IsCurrentMatchHost)
                    {
                        FinishGame();
                    }
                    else
                    {
                        ExitGame();
                    }
                    UnsubscribeToActiveUsersList();
                    ExitChatRoom();
                    ClearCommunicationChannels();
                    RedirectPermanentlyToMainMenu();
                }
            }
        }

        private void ShowErrorSendingMessage(ResponseStatus errorStatus)
        {
            switch (errorStatus)
            {
                case ResponseStatus.VALIDATION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbSendMessageMatchFinishedMessage,
                        Properties.Resources.msgbInvalidMatchCodeTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
                case ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbMesageNotSentMessage,
                        Properties.Resources.msgbMessageNotSentTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
                default:
                    MessageBox.Show(
                        ServerResponse.GetMessageFromStatusCode(errorStatus),
                        Properties.Resources.msgbInvalidMatchCodeTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
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
            string adversaryNickname = string.IsNullOrEmpty(gameManager.AdversaryNickname) ? defaultAdversaryNickname : gameManager.AdversaryNickname;

            ShowAdversaryMessageInChat(message, adversaryNickname);
        }

        private void ShowAdversaryMessageInChat(string message, string adversaryNickname)
        {
            ChatMessage messageElement = new ChatMessage();
            messageElement.TbMessage.Text = message;
            messageElement.TbNickname.Text = adversaryNickname;

            SpChatMessages.Children.Add(messageElement);
            SvChatMessages.ScrollToBottom();
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
            }

            RedirectPermanentlyToMainMenu();
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

            StopListeningMatchStatus();
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
            }

            RedirectPermanentlyToMainMenu();
        }

        private void FinishGame()
        {
            gameManager.Client.FinishGame(gameManager.CurrentMatchCode);
            gameManager.RestartRawValues();

            StopListeningMatchStatus();
        }

        private void BtnStartGameClick(object sender, RoutedEventArgs e)
        {
            try
            {
                UnsubscribeToActiveUsersList();
                ExitChatRoom();
                StartCharacterSelection();
                NavigateToChooseCharacterPage();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void StartCharacterSelection()
        {
            matchStatusManager.Client.StartCharacterSelection(matchStatusManager.CurrentMatchCode);
        }

        private void BtnInviteToGameClick(object sender, RoutedEventArgs e)
        {
            Button invitationButton = e.Source as Button;
            string activeUserNickname = (string)invitationButton.DataContext;

            string nickname = activeUserNickname;
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
                ClearCommunicationChannels();
                RedirectPermanentlyToMainMenu();
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