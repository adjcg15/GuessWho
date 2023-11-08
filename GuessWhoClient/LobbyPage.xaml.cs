using GuessWhoClient.GameServices;
using GuessWhoClient.Properties;
using GuessWhoClient.Utils;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GuessWhoClient
{
    public partial class LobbyPage : Page, IUserServiceCallback, IMatchServiceCallback
    {
        private string invitationCode;
        private UserServiceClient userServiceClient;
        private MatchServiceClient matchServiceClient;
        public ObservableCollection<ActiveUser> activeUsers { get; set; } = new ObservableCollection<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            userServiceClient = new UserServiceClient(new InstanceContext(this));
            matchServiceClient = new MatchServiceClient(new InstanceContext(this));

            userServiceClient.Subscribe();
            var createMatchResponse = matchServiceClient.CreateMatch(userNickname);

            if (createMatchResponse.StatusCode == ResponseStatus.OK)
            {
                this.invitationCode = createMatchResponse.Value;
            }
            else
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(createMatchResponse.StatusCode),
                    resourceManager.GetString("msgbErrorCreatingMatchTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            ShowActiveUsers(userNickname);
        }

        public LobbyPage(string invitationCode)
        {
            InitializeComponent();
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);
            this.invitationCode = invitationCode;
            string userNickname = DataStore.Profile != null ? DataStore.Profile.NickName : "";
            userServiceClient = new UserServiceClient(new InstanceContext(this));
            matchServiceClient = new MatchServiceClient(new InstanceContext(this));

            userServiceClient.Subscribe();
            var joinGameResponse = matchServiceClient.JoinGame(invitationCode, userNickname);

            if (joinGameResponse.StatusCode == ResponseStatus.OK)
            {
                PlayerInMatch host = joinGameResponse.Value;

                ShowUserInfoInBanner(host.Nickname, host.Avatar);
                ShowUserInfoInChat(userNickname, host.Avatar);
            }
            else
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(joinGameResponse.StatusCode),
                    resourceManager.GetString("msgbErrorJoiningMatchTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }

            ShowActiveUsers(userNickname);
        }

        private void ShowActiveUsers(string userNickname)
        {
            activeUsers = new ObservableCollection<ActiveUser>(userServiceClient.GetActiveUsers().ToList());
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
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch user, bool isInMatch)
        {
            if(user.IsHost)
            {

            }
            else
            {
                ShowGuestInformation(user);
            }
        }

        private void ShowGuestInformation(PlayerInMatch guest)
        {
            ShowUserInfoInBanner(guest.Nickname, guest.Avatar);
            ShowUserInfoInChat(guest.Nickname, guest.Avatar);
        }

        private void ShowUserInfoInBanner(string nickname, byte[] avatar)
        {
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);

            BorderOponent.Background = new SolidColorBrush(Color.FromRgb(182, 216, 242));
            TbOponent.Text = nickname != "" ? nickname : resourceManager.GetString("txtGuest");
            if (avatar != null)
            {
                ImgProfilePicture.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(avatar);
            }
        }

        private void ShowUserInfoInChat(string nickname, byte[] avatar)
        {
            ResourceManager resourceManager = new ResourceManager("GuessWhoClient.Properties.Resources", typeof(Resources).Assembly);

            TbOponentChat.Text = nickname != "" ? nickname : resourceManager.GetString("txtGuest");
            if (avatar != null)
            {
                ImgChatProfilePicture.ImageSource = ImageTransformator.GetBitmapImageFromByteArray(avatar);
            }
        }

        private void BtnCopyInvitationCodeClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(invitationCode);
        }

        private void ClosingPage(object sender, RoutedEventArgs e)
        {
            userServiceClient.Unsubscribe();
        }

        private void ListBoxActiveUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListBoxActiveUsers_SelectionChanged_1()
        {

        }
    }
}
