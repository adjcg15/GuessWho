using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Lógica de interacción para AnswerPage.xaml
    /// </summary>
    public partial class AnswerPage : Page, IGamePage, IMatchStatusPage
    {
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;

        public AnswerPage()
        {
            InitializeComponent();
        }

        public void PaintOpponentDraw(List<Line> opponentDraw)
        {
            foreach (var line in opponentDraw)
            {
                CnvsOpponentDraw.Children.Add(line);
            }
        }

        private void BtnReportPlayerClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAnswerNoClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnAnswerYesClick(object sender, RoutedEventArgs e)
        {

        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            throw new NotImplementedException();
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            throw new NotImplementedException();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
