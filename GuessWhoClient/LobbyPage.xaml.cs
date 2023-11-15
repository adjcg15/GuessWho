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
    public partial class LobbyPage : Page, IUserServiceCallback, IMatchServiceCallback
    {
        private const string DEFAULT_PROFILE_PICTURE_ROUTE = "pack://application:,,,/Resources/user-icon.png";
        private string invitationCode;
        private bool isHost;
        private MatchServiceClient matchServiceClient;
        public ObservableCollection<ActiveUser> activeUsers { get; set; } = new ObservableCollection<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();
            isHost = true;
            
            BtnCancelGame.Visibility = Visibility.Visible;
            BtnStartGame.Visibility = Visibility.Visible;
        }

        public LobbyPage(string invitationCode)
        {
            InitializeComponent();
            isHost = false;
            this.invitationCode = invitationCode;

            BtnExitGame.Visibility = Visibility.Visible;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            matchServiceClient = new MatchServiceClient(new InstanceContext(this));
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            DataStore.OpenUserServiceClientChannel(this);

            try
            {
                SubscribeToActiveUsersList();
                if (isHost)
                {
                    CreateNewGame(userNickname);
                    ShowActiveUsers(userNickname);
                    ShowActiveUsersList();
                }
                else
                {
                    JoinGame(userNickname);
                    ShowActiveUsers(userNickname);
                    ShowGameFullMessage();
                }
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(
                    Properties.Resources.msgbErrorConexionServidorMessage,
                    Properties.Resources.msgbErrorConexionServidorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                RedirectPermanentlyToMainMenu();
            }
        }

        private void SubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Subscribe();
        }

        private void CreateNewGame(string userNickname)
        {
            var createMatchResponse = matchServiceClient.CreateMatch(userNickname);
            invitationCode = createMatchResponse.Value;
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
            var joinGameResponse = matchServiceClient.JoinGame(invitationCode, userNickname);
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
            LbUsersListMessage.Content = Properties.Resources.lbAllPlayersReady;
            LbUsersListMessage.Visibility = Visibility.Visible;

            ListBoxActiveUsers.Visibility = Visibility.Hidden;
        }

        private void RedirectPermanentlyToMainMenu()
        {
            ShowsNavigationUI = true;
            MainMenuPage menuPage = new MainMenuPage();
            NavigationService.Navigate(menuPage);
        }

        private void ShowActiveUsers(string userNickname)
        {
            activeUsers = new ObservableCollection<ActiveUser>(DataStore.UsersClient.GetActiveUsers().ToList());
            if (DataStore.Profile != null)
            {
                activeUsers.Remove(activeUsers.FirstOrDefault(u => u.Nickname == userNickname));
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
            if(user.IsHost)
            {
                if(!isJoiningMatch)
                {
                    FinishGameForGuest();
                }
            }
            else
            {
                if (isJoiningMatch)
                {
                    ShowGuestInformation(user);
                    ShowGameFullMessage();
                }
                else
                {
                    HideGuestInformation();
                    ShowActiveUsersList();
                }
            }
        }

        private void FinishGameForGuest()
        {
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

        private void BtnSendMessageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMessage();
            }
            catch (EndpointNotFoundException)
            {
                MessageBox.Show(
                    Properties.Resources.msgbErrorConexionServidorMessage,
                    Properties.Resources.msgbErrorConexionServidorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                RedirectPermanentlyToMainMenu();
            }
        }

        private void SendMessage()
        {
            string message = TbMessage.Text;

            if (!string.IsNullOrEmpty(message))
            {
                var response = matchServiceClient.SendMessage(invitationCode, message);

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
                        RedirectPermanentlyToMainMenu();
                        break;
                    case ResponseStatus.CLIENT_CHANNEL_CONNECTION_ERROR:
                        MessageBox.Show(
                            Properties.Resources.msgbMesageNotSentMessage,
                            Properties.Resources.msgbMessageNotSentTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        if(!isHost)
                        {
                            RedirectPermanentlyToMainMenu();
                        }
                        else
                        {
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

        public void NotifyNewMessage(string message, string senderNickname)
        {
            string defaultAdversaryNickname = isHost ? Properties.Resources.txtGuest : Properties.Resources.txtHost;
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
            Clipboard.SetText(invitationCode);
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
                MessageBox.Show(
                    Properties.Resources.msgbErrorConexionServidorMessage,
                    Properties.Resources.msgbErrorConexionServidorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                RedirectPermanentlyToMainMenu();
            }
        }

        private void UnsubscribeToActiveUsersList()
        {
            DataStore.UsersClient.Unsubscribe();
            ExitGame();
        }

        private void ExitGame()
        {
            matchServiceClient.ExitGame(invitationCode);
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
                MessageBox.Show(
                    Properties.Resources.msgbErrorConexionServidorMessage,
                    Properties.Resources.msgbErrorConexionServidorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                RedirectPermanentlyToMainMenu();
            }
        }

        private void FinishGame()
        {
            matchServiceClient.FinishGame(invitationCode);
            RedirectPermanentlyToMainMenu();
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
                MessageBox.Show(
                    Properties.Resources.msgbErrorConexionServidorMessage,
                    Properties.Resources.msgbErrorConexionServidorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
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
                Properties.Resources.txtInviteToGameBody + invitationCode
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