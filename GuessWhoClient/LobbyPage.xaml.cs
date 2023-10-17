using GuessWhoClient.GameServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GuessWhoClient
{
    /// <summary>
    /// Lógica de interacción para LobbyPage.xaml
    /// </summary>
    public partial class LobbyPage : Page, GameServices.IUserServiceCallback
    {
        private static List<ActiveUser> activeUsers = new List<ActiveUser>();

        public LobbyPage()
        {
            InitializeComponent();

            InstanceContext context = new InstanceContext(this);
            GameServices.UserServiceClient userServiceClient = new GameServices.UserServiceClient(context);

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
            InstanceContext context = new InstanceContext(this);
            UserServiceClient userServiceClient = new UserServiceClient(context);

            Console.WriteLine("Saliendo de la página");
            userServiceClient.Unsubscribe();
        }
    }
}
