using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GuessWhoClient
{
    public class Player
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Score { get; set; }
    }

    public partial class ScoreboardPage : Page
    {
        private ObservableCollection<GameServices.TopPlayer> topPlayers { get; set; }
        
        public ScoreboardPage()
        {
            InitializeComponent();
            LoadBestPlayers("", 100);
        }

        private void BtnSearchPlayersClick(object sender, RoutedEventArgs e)
        {
            string query = TbSearchQuery.Text;
            LoadBestPlayers(query, 10);
        }

        private void LoadBestPlayers(string query, int numberOfPlayers)
        {
            var userServiceClient = new GameServices.PlayerServiceClient();
            var response = userServiceClient.GetBestPlayers(query, numberOfPlayers);

            if (response.StatusCode == GameServices.ResponseStatus.OK)
            {
                if(response.Value.Length > 0)
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
    }
}
