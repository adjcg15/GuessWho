using GuessWhoClient.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class LobbyPage : Page, GameServices.IUserServiceCallback
    {
        private GameServices.UserServiceClient userServiceClient;
        private static List<ActiveUser> activeUsers = new List<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();

            userServiceClient = new GameServices.UserServiceClient(new InstanceContext(this));
            userServiceClient.Subscribe();
            activeUsers = userServiceClient.GetActiveUsers().ToList();

            ListBoxActiveUsers.ItemsSource = activeUsers;
        }

        public void UserStatusChanged(ActiveUser user, bool isActive)
        {
            if (isActive)
            {
                activeUsers.Add(user);
            }
            else
            {
                activeUsers.Remove(activeUsers.Find(u => u.Nickname == user.Nickname));
            }
            ListBoxActiveUsers.ItemsSource = activeUsers;
        }

        private void ClosingPage(object sender, RoutedEventArgs e)
        {
            userServiceClient.Unsubscribe();
        }
    }
}
