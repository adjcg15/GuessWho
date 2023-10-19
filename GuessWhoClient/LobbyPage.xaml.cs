using GuessWhoClient.GameServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class LobbyPage : Page, GameServices.IUserServiceCallback
    {
        private GameServices.UserServiceClient userServiceClient;
        public ObservableCollection<ActiveUser> activeUsers { get; set; } = new ObservableCollection<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();

            userServiceClient = new GameServices.UserServiceClient(new InstanceContext(this));
            userServiceClient.Subscribe();
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

        private void ClosingPage(object sender, RoutedEventArgs e)
        {
            userServiceClient.Unsubscribe();
        }
    }
}
