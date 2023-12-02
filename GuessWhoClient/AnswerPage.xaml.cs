using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GuessWhoClient
{
    public partial class AnswerPage : Page, IGamePage, IMatchStatusPage
    {
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;
        private const int PEN_THICKNESS = 4;

        public AnswerPage(SerializedLine[] draw)
        {
            InitializeComponent();
            PaintDrawInCanvas(draw);
        }

        private void PaintDrawInCanvas(SerializedLine[] draw)
        {
            foreach (var line in UnserializeDraw(draw))
            {
                if (!CnvsOpponentDraw.Children.Contains(line))
                {
                    CnvsOpponentDraw.Children.Add(line);
                }
            }
        }

        private List<Line> UnserializeDraw(SerializedLine[] adversaryDrawMap)
        {
            List<Line> drawReceived = new List<Line>();

            foreach (var serializedLine in adversaryDrawMap)
            {
                var line = new Line
                {
                    X1 = serializedLine.StartPoint.X,
                    Y1 = serializedLine.StartPoint.Y,
                    X2 = serializedLine.EndPoint.X,
                    Y2 = serializedLine.EndPoint.Y,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(serializedLine.Color)),
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeThickness = PEN_THICKNESS
                };

                drawReceived.Add(line);
            }

            return drawReceived;
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
