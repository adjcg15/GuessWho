using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.ServiceModel;
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
        private bool reportFinished;
        private bool ownAnswerSent;
        private bool opponentAnswerReceived;
        private bool opponentDrawingLooksLikeAnswer;

        public AnswerPage()
        {
            InitializeComponent();
        }

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

        private List<Polyline> UnserializeDraw(SerializedLine[] adversaryDrawMap)
        {
            List<Polyline> drawReceived = new List<Polyline>();

            foreach (SerializedLine serializedPolyline in adversaryDrawMap)
            {
                var line = new Polyline
                {
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(serializedPolyline.Color)),
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeThickness = PEN_THICKNESS
                };

                foreach (SerializedPoint point in serializedPolyline.Points)
                {
                    line.Points.Add(new Point
                    {
                        X = point.X,
                        Y = point.Y
                    });
                }

                drawReceived.Add(line);
            }

            return drawReceived;
        }

        private void BtnReportPlayerClick(object sender, RoutedEventArgs e)
        {
            BorderOpacityReport.Visibility = Visibility.Visible;
            BorderReport.Visibility = Visibility.Visible;
        }

        private void BtnAnswerNoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SendClueToOpponent(false);
                DisableAnswerButtons();
                ShowWaitingAnswerMessage();
            }
            catch(EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();

                gameManager.UnsubscribePage(this);
                matchStatusManager.UnsubscribePage(this);
                gameManager.RestartRawValues();
                matchStatusManager.RestartRawValues();

                RedirectToMainMenuFromCanceledMatch();
            }
        }

        private void BtnAnswerYesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SendClueToOpponent(true);
                DisableAnswerButtons();
                ShowWaitingAnswerMessage();
            }
            catch(EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();

                gameManager.UnsubscribePage(this);
                matchStatusManager.UnsubscribePage(this);
                gameManager.RestartRawValues();
                matchStatusManager.RestartRawValues();

                RedirectToMainMenuFromCanceledMatch();
            }
        }

        private void SendClueToOpponent(bool looksLike)
        {
            matchStatusManager.Client.SendAnswer(looksLike, gameManager.CurrentMatchCode);
            ownAnswerSent = true;

            if (opponentAnswerReceived)
            {
                if(opponentDrawingLooksLikeAnswer)
                {
                    RedirectToGamePageFromSimilarDrawingClue();
                }
                else
                {

                    RedirectToGamePageFromNotSimilarDrawingClue();
                }
            }
        }

        private void DisableAnswerButtons()
        {
            BtnAnswerNo.IsEnabled = false;
            BtnAnswerYes.IsEnabled = false;
        }

        private void ShowWaitingAnswerMessage()
        {
            TbWaitingAnswer.Visibility = Visibility.Visible;
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            switch (matchStatusCode)
            {
                case MatchStatus.LooksLike:
                    opponentAnswerReceived = true;
                    opponentDrawingLooksLikeAnswer = true;

                    if (ownAnswerSent)
                    {
                        RedirectToGamePageFromSimilarDrawingClue();
                    }
                    break;
                case MatchStatus.DoesNotLookLike:
                    opponentAnswerReceived = true;
                    opponentDrawingLooksLikeAnswer = false;

                    if (ownAnswerSent)
                    {
                        RedirectToGamePageFromNotSimilarDrawingClue();
                    }
                    break;
            }
        }

        private void RedirectToGamePageFromSimilarDrawingClue()
        {
            DrawingPage gamePage = new DrawingPage();
            gamePage.ShowClueSimilarDrawing();
            NavigationService.Navigate(gamePage);
        }

        private void RedirectToGamePageFromNotSimilarDrawingClue()
        {
            DrawingPage gamePage = new DrawingPage();
            gamePage.ShowClueNotSimilarDrawing();
            NavigationService.Navigate(gamePage);
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            ClearCommunicationChannels();
            RedirectToMainMenuFromCanceledMatch();
        }

        private void ClearCommunicationChannels()
        {
            if (!gameManager.IsCurrentMatchHost)
            {
                gameManager.Client.ExitGame(gameManager.CurrentMatchCode);
            }
            else
            {
                gameManager.Client.FinishGame(gameManager.CurrentMatchCode);
            }
            matchStatusManager.Client.StopListeningMatchStatus(matchStatusManager.CurrentMatchCode);

            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);

            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
        }

        private void RedirectToMainMenuFromCanceledMatch()
        {
            MainMenuPage mainMenu = new MainMenuPage();
            mainMenu.ShowCanceledMatchMessage();
            NavigationService.Navigate(mainMenu);
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if(gameManager.AdversaryNickname == string.Empty || DataStore.Profile == null)
            {
                BtnReportPlayer.Visibility = Visibility.Collapsed;
            }
        }

        private void TbReportCommentGotFocus(object sender, RoutedEventArgs e)
        {
            if (TbReportComment.Text == Properties.Resources.tbReportCommentPlaceholder)
            {
                TbReportComment.Text = "";
                TbReportComment.Foreground = Brushes.Black;
            }
        }

        private void BtnReport_ExitClick(object sender, RoutedEventArgs e)
        {
            if (!reportFinished)
            {
                if (ValidateForm())
                {
                    SendReport();
                }
            }
            else
            {
                ShowsNavigationUI = true;
                MainMenuPage mainMenu = new MainMenuPage();
                NavigationService.Navigate(mainMenu);
                matchStatusManager.UnsubscribePage(this);
                gameManager.UnsubscribePage(this);
                ClearCommunicationChannels();
            }
        }


        private bool ValidateForm()
        {
            bool isValid = true;
            TbReportCommentBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FFABADB3");
            CbReportReasonBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FFABADB3");

            Console.WriteLine(CbReportReason.SelectedIndex + " " + TbReportComment.Text);
            if(CbReportReason.SelectedIndex == -1 && (TbReportComment.Text == string.Empty || TbReportComment.Text == Properties.Resources.tbReportCommentPlaceholder)) 
            {
                TbReportCommentBorder.BorderBrush = Brushes.Red;
                CbReportReasonBorder.BorderBrush = Brushes.Red;
                MessageBox.Show(Properties.Resources.msgbEmptyReportFields);
                isValid = false;
            }
            else if (CbReportReason.SelectedIndex == -1)
            {
                CbReportReasonBorder.BorderBrush = Brushes.Red;
                MessageBox.Show(Properties.Resources.msgbEmptyReportReason);
                isValid = false;
            }
            else if(TbReportComment.Text == string.Empty || TbReportComment.Text == Properties.Resources.tbReportCommentPlaceholder)
            {
                TbReportCommentBorder.BorderBrush = Brushes.Red;
                MessageBox.Show(Properties.Resources.msgbEmptyReportComment);
                isValid = false;
            }

            return isValid;
        }

        private void SendReport()
        {
            ReportServiceClient reportServiceClient = new ReportServiceClient();

            PlayerReport playerReport = new PlayerReport
            {
                IdReportType = CbReportReason.SelectedIndex + 1,
                NicknameReported = gameManager.AdversaryNickname,
                ReportComment = TbReportComment.Text
            };


            try
            {
                var response = reportServiceClient.ReportPlayer(playerReport);

                if (response.StatusCode == ResponseStatus.OK)
                {
                    if (response.Value)
                    {
                        ShowMessageAfterReport();
                        ChangeButtons();
                    }
                }
                else
                {
                    ShowErrorSendingReport(response.StatusCode);
                }
            }
            catch (EndpointNotFoundException)
            {
                ServerResponse.ShowServerDownMessage();
            }
        }

        private void ShowErrorSendingReport(ResponseStatus errorStatus)
        {
            switch (errorStatus)
            {
                case ResponseStatus.VALIDATION_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbReportUserNotFoundMessage,
                        Properties.Resources.msgbReportUserNotFoundTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
                case ResponseStatus.UPDATE_ERROR:
                    MessageBox.Show(
                        Properties.Resources.msgbReportErrorUpdateMessage,
                        Properties.Resources.msgbReportErrorUpdateTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
                default:
                    MessageBox.Show(
                        ServerResponse.GetMessageFromStatusCode(errorStatus),
                        ServerResponse.GetMessageFromStatusCode(errorStatus),
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    break;
            }
        }

        private void ShowMessageAfterReport()
        {
            GridReportComment.Visibility = Visibility.Collapsed;
            LbReportComment.Visibility = Visibility.Collapsed;
            GridReportReason.Visibility = Visibility.Collapsed;
            LbReportReason_MessageBoxMessage.Content = Properties.Resources.lbReportFinishedMessage;
            LbMessageBoxTitle.Content = Properties.Resources.lbTitleReportFinished;
        }

        private void ChangeButtons()
        {
            reportFinished = true;
            BtnCancelReport_Continue.Content = Properties.Resources.btnContinueAfterReport;
            BtnReport_Exit.Content = Properties.Resources.btnLeave;
        }

        private void BtnCancelReport_ContinueClick(object sender, RoutedEventArgs e)
        {
            if (!reportFinished)
            {
                CloseReportWindow();
                ResetReportWindowValues();
            }
            else
            {
                CloseReportWindow();
                BtnReportPlayer.IsEnabled = false;
                BtnReportPlayer.Opacity = 0.6;
            }
        }

        private void CloseReportWindow()
        {
            BorderOpacityReport.Visibility = Visibility.Collapsed;
            BorderReport.Visibility = Visibility.Collapsed;
        }

        private void ResetReportWindowValues()
        {
            LbMessageBoxTitle.Content = Properties.Resources.txtReportPlayer;
            LbReportReason_MessageBoxMessage.Content = Properties.Resources.lbReportReason;

            GridReportReason.Visibility = Visibility.Visible;
            CbReportReason.SelectedIndex = -1;
            CbReportReason.Text = Properties.Resources.cbReportReasonPlaceholder;
            CbReportReasonBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FFABADB3");

            GridReportComment.Visibility = Visibility.Visible;
            TbReportComment.Text = Properties.Resources.tbReportCommentPlaceholder;
            TbReportComment.Foreground = (Brush)new BrushConverter().ConvertFrom("#95000000");
            TbReportCommentBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FFABADB3");

            BtnCancelReport_Continue.Content = Properties.Resources.btnCancelReport;
            BtnReport_Exit.Content = Properties.Resources.txtReportGlobal;
            reportFinished = false;
        }

        private void TbReportCommentTextChanged(object sender, TextChangedEventArgs e)
        {
            if(TbReportComment.Text == string.Empty)
            {
                TbReportCommentBorder.BorderBrush = Brushes.Red;
            }
            else
            {
                TbReportCommentBorder.BorderBrush = (Brush) new BrushConverter().ConvertFrom("#FFABADB3");
            }

            if (TbReportComment.Text.Length > 255)
            {
                TbReportComment.Text = TbReportComment.Text.Substring(0, 255);

                TbReportComment.CaretIndex = TbReportComment.Text.Length;
            }
        }

        private void CbReportReasonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CbReportReason.SelectedIndex != -1)
            {
                CbReportReasonBorder.BorderBrush = (Brush)new BrushConverter().ConvertFrom("#FFABADB3");
            }
        }
    }
}
