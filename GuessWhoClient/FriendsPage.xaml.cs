using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GuessWhoClient
{
    /// <summary>
    /// Lógica de interacción para FriendsPage.xaml
    /// </summary>
    public partial class FriendsPage : Page
    {
        private DispatcherTimer messageTimer;
        private const int MESSAGE_DISPLAY_DURATION = 3000;

        private ObservableCollection<Friend> requests = new ObservableCollection<Friend>();
        private ObservableCollection<Friend> friends = new ObservableCollection<Friend>();

        public FriendsPage()
        {
            InitializeComponent();

            UploadRequests();
            UploadFriends();
        }

        private void UploadFriends()
        {
            FriendsServiceClient friendsServiceClient = new FriendsServiceClient();
            friends = new ObservableCollection<Friend>(friendsServiceClient.GetFriends(DataStore.Profile.IdUser).Value);

            friends.OrderByDescending(friend => friend.Status == "Online").ToList();

            ListBoxFriends.ItemsSource = friends;
        }

        private void UploadRequests()
        {
            FriendsServiceClient friendsServiceClient = new FriendsServiceClient();
            requests = new ObservableCollection<Friend>(friendsServiceClient.GetRequests(DataStore.Profile.IdUser).Value);

            ListBoxFriendRequests.ItemsSource = requests;
        }

        private void BtnAcceptInvitationClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button) sender;
            Friend friend = (Friend) button.DataContext;
            
            AnswerRequest(friend, "Accept");            
        }

        private void BtnDeclineInvitationClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Friend friend = (Friend)button.DataContext;

            AnswerRequest(friend, "Decline");
        }

        private void AnswerRequest(Friend requester, string answer)
        {
            FriendsServiceClient friendsServiceClient = new FriendsServiceClient();
            if (friendsServiceClient.AnswerRequest(requester.IdFriendship, answer).Value)
            {
                requests.Remove(requester);
            }
        }

        private void BtnAddFriendClick(object sender, RoutedEventArgs e) 
        {
            string nicknameRequested = TbNameRequest.Text.Trim();

            if (nicknameRequested == "")
            {
                TbMessage.Text = Properties.Resources.lbStatusRequestFailed;
                TbMessage.Visibility = Visibility.Visible;

                StartAnimationTbMessage();
            }
            else
            {
                AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
                ProfileResponse profileResponse = authenticationServiceClient.VerifyUserRegisteredByNickName(nicknameRequested);
                
                if(profileResponse.StatusCode == ResponseStatus.OK && profileResponse.Value == null) 
                {
                    TbMessage.Text = Properties.Resources.lbStatusRequestFailed;
                    TbMessage.Visibility = Visibility.Visible;

                    StartAnimationTbMessage();
                }
                else
                {
                    FriendsServiceClient friendsServiceClient = new FriendsServiceClient();
                    booleanResponse booleanResponse = friendsServiceClient.SendRequest(DataStore.Profile.IdUser, profileResponse.Value.IdUser);

                    if(booleanResponse.Value)
                    {
                        TbMessage.Text = Properties.Resources.lbStatusRequestSuccess;
                        TbMessage.Visibility = Visibility.Visible;
                        StartAnimationTbMessage();
                    }
                }

            }
        }

        private void StartAnimationTbMessage()
        {
            messageTimer = new DispatcherTimer();
            messageTimer.Tick += (timerSender, args) =>
            {
                TbMessage.Visibility = Visibility.Hidden;

                TbNameRequest.Text = "";

                messageTimer.Stop();
            };
            messageTimer.Interval = TimeSpan.FromMilliseconds(MESSAGE_DISPLAY_DURATION);
            messageTimer.Start();

            DoubleAnimation animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(MESSAGE_DISPLAY_DURATION));
            TbMessage.BeginAnimation(OpacityProperty, animation);
        }
    }
}
