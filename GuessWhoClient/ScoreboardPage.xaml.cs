using GuessWhoClient.GameServices;
using GuessWhoClient.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public partial class ScoreboardPage : Page
    {
        private ObservableCollection<TopPlayer> topPlayers { get; set; }
        
        public ScoreboardPage()
        {
            InitializeComponent();
        }

        private void BtnSearchPlayersClick(object sender, RoutedEventArgs e)
        {
            string query = TbSearchQuery.Text;
            LoadBestPlayers(query, 10);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            LoadBestPlayers("", 100);
        }

        private void LoadBestPlayers(string query, int numberOfPlayers)
        {
            try
            {
                var userServiceClient = new PlayerServiceClient();
                var response = userServiceClient.GetBestPlayers(query, numberOfPlayers);

                if (response.StatusCode == ResponseStatus.OK)
                {
                    if (response.Value.Length > 0)
                    {
                        topPlayers = new ObservableCollection<GameServices.TopPlayer>(response.Value.ToList());
                        DataGridPlayers.ItemsSource = topPlayers;
                        ShowScoreboard();
                    }
                    else
                    {
                        ShowEmptyScoreboardMessage();
                    }
                }
                else
                {
                    ShowEmptyScoreboardMessage();
                    MessageBox.Show(
                        ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                        ServerResponse.GetTitleFromStatusCode(response.StatusCode),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                }
            }
            catch (EndpointNotFoundException ex)
            {
                ServerResponse.ShowServerDownMessage();
                App.log.Fatal(ex.Message);
                RedirectToMainMenu();
            }
            catch (CommunicationException ex)
            {
                ServerResponse.ShowConnectionLostMessage();
                App.log.Error(ex.Message);
                RedirectToMainMenu();
            }
        }

        private void RedirectToMainMenu()
        {
            MainMenuPage mainMenu = new MainMenuPage();
            NavigationService.Navigate(mainMenu);
        }

        private void ShowEmptyScoreboardMessage()
        {
            DataGridPlayers.Visibility = Visibility.Collapsed;
            LbEmptyScoreboard.Visibility = Visibility.Visible;
        }

        private void ShowScoreboard()
        {
            DataGridPlayers.Visibility = Visibility.Visible;
            LbEmptyScoreboard.Visibility = Visibility.Collapsed;
        }

        private void BtnReturnPreviousPageClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
