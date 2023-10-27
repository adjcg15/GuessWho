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

    public partial class Scoreboard : Page
    {
        private ObservableCollection<GameServices.TopPlayer> topPlayers { get; set; }
        
        public Scoreboard()
        {
            InitializeComponent();
            LoadBestPlayers();
        }

        private void LoadBestPlayers()
        {
            var userServiceClient = new GameServices.PlayerServiceClient();
            var response = userServiceClient.GetBestPlayers(10);

            if (response.StatusCode == GameServices.ResponseStatus.OK)
            {
                topPlayers = new ObservableCollection<GameServices.TopPlayer>(response.Value.ToList());
                DataGridPlayers.ItemsSource = topPlayers;
            }
            else
            {
                MessageBox.Show(
                    ServerResponse.GetMessageFromStatusCode(response.StatusCode),
                    ServerResponse.GetTitleFromStatusCode(response.StatusCode),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }
    }
}
