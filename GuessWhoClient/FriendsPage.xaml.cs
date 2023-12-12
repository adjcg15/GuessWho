using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const string ONLINE_STATUS = "Online";
        private const string ACCEPT_REQUEST = "Accept";
        private const string DECLINE_REQUEST = "Decline";
        private DispatcherTimer messageTimer;
        private const int MESSAGE_DISPLAY_DURATION = 3000;

        private ObservableCollection<Friend> requests = new ObservableCollection<Friend>();
        private ObservableCollection<Friend> friends = new ObservableCollection<Friend>();

        public FriendsPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UploadRequests();
                UploadFriends();
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
                RedirectPermanentlyToMainMenu();
            }
        }

        private void UploadFriends()
        {
            FriendsServiceClient friendsServiceClient = new FriendsServiceClient();
            friends = new ObservableCollection<Friend>(friendsServiceClient.GetFriends(DataStore.Profile.IdUser).Value);

            friends.OrderByDescending(friend => friend.Status == ONLINE_STATUS).ToList();

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
            
            AnswerRequest(friend, ACCEPT_REQUEST);            
        }

        private void BtnDeclineInvitationClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Friend friend = (Friend)button.DataContext;

            AnswerRequest(friend, DECLINE_REQUEST);
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

            if (string.IsNullOrEmpty(nicknameRequested))
            {
                TbMessage.Text = Properties.Resources.lbStatusRequestFailed;
                StartAnimationTbMessage();
            }
            else
            {
                AuthenticationServiceClient authenticationServiceClient = new AuthenticationServiceClient();
                ProfileResponse profileResponse = authenticationServiceClient.VerifyUserRegisteredByNickName(nicknameRequested);

                if (profileResponse.StatusCode == ResponseStatus.OK && profileResponse.Value.IdUser == 0)
                {
                    TbMessage.Text = Properties.Resources.lbStatusRequestFailed;
                    StartAnimationTbMessage();
                }
                else
                {
                    FriendsServiceClient friendsServiceClient = new FriendsServiceClient();

                    if (friends.Any(friend => friend.Nickname == nicknameRequested))
                    {
                        TbMessage.Text = Properties.Resources.txtAlreadyFriends;
                        StartAnimationTbMessage();
                    }
                    else if (nicknameRequested == DataStore.Profile.NickName)
                    {
                        TbMessage.Text = Properties.Resources.txtOwnNickname;
                        StartAnimationTbMessage();
                    }
                    else
                    {
                        booleanResponse booleanResponse = friendsServiceClient.SendRequest(DataStore.Profile.IdUser, profileResponse.Value.IdUser);

                        if (booleanResponse.Value)
                        {
                            TbMessage.Text = Properties.Resources.lbStatusRequestSuccess;
                            StartAnimationTbMessage();
                        }
                        else if(booleanResponse.StatusCode == ResponseStatus.NOT_ALLOWED)
                        {
                            TbMessage.Text = Properties.Resources.txtAlreadyRequested;
                            StartAnimationTbMessage();
                        }
                        else
                        {
                            TbMessage.Text = ServerResponse.GetTitleFromStatusCode(booleanResponse.StatusCode);
                            StartAnimationTbMessage();
                        }
                    }
                }
            }
        }

        private void StartAnimationTbMessage()
        {
            TbNameRequest.Text = "";
            TbNameRequest.IsReadOnly = true;

            TbMessage.Visibility = Visibility.Visible;
            messageTimer = new DispatcherTimer();
            messageTimer.Tick += (timerSender, args) =>
            {
                TbNameRequest.IsReadOnly = false; 
                TbMessage.Visibility = Visibility.Hidden;

                TbNameRequest.Text = "";

                messageTimer.Stop();
            };
            messageTimer.Interval = TimeSpan.FromMilliseconds(MESSAGE_DISPLAY_DURATION);
            messageTimer.Start();

            DoubleAnimation animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(MESSAGE_DISPLAY_DURATION));
            TbMessage.BeginAnimation(OpacityProperty, animation);
        }

        private void RedirectPermanentlyToMainMenu()
        {
            ShowsNavigationUI = true;
            MainMenuPage mainMenuPage = new MainMenuPage();
            NavigationService.Navigate(mainMenuPage);
        }
    }
}
