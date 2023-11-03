using GuessWhoClient.GameServices;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class LobbyPage : Page, IUserServiceCallback, IMatchServiceCallback
    {
        private static string invitationCode;
        private UserServiceClient userServiceClient;
        private MatchServiceClient matchServiceClient;
        public ObservableCollection<ActiveUser> activeUsers { get; set; } = new ObservableCollection<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();

            userServiceClient = new UserServiceClient(new InstanceContext(this));
            matchServiceClient = new MatchServiceClient(new InstanceContext(this));
            userServiceClient.Subscribe();
            matchServiceClient.CreateMatch(DataStore.Profile != null ? DataStore.Profile.NickName : "");

            activeUsers = new ObservableCollection<ActiveUser>(userServiceClient.GetActiveUsers().ToList());
            if (DataStore.Profile != null)
            {
                string nickname = DataStore.Profile.NickName;
                activeUsers.Remove(activeUsers.FirstOrDefault(u => u.Nickname == nickname));
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
            throw new System.NotImplementedException();
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
