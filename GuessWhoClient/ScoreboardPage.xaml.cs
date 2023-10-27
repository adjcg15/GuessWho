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
        public Scoreboard()
        {
            InitializeComponent();

            ObservableCollection<Player> players = new ObservableCollection<Player>();
            for(int i = 1; i < 16; i++)
            {
                players.Add(new Player { 
                    Name = "Player " + i, 
                    Position = (16 - i).ToString(), 
                    Score=((16-i) * 100).ToString()
                });
            }
            DataGridPlayers.ItemsSource = players;
        }
    }
}
