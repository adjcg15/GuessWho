﻿using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using GuessWhoClient.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GuessWhoClient
{
    public partial class DrawingPage : Page, INotifyPropertyChanged, IGamePage, IMatchStatusPage, IDrawServiceCallback
    {
        private bool isInteractingWithCanvas;
        private bool isDrawingMode = true;
        private bool isChoosingCharacter;
        private string selectedColor = "#000000";
        private const int PEN_THICKNESS = 4;
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly GameManager gameManager = GameManager.Instance;
        private readonly MatchStatusManager matchStatusManager = MatchStatusManager.Instance;
        private DrawServiceClient drawServiceClient;
        private Timer timer;
        private int secondsRemaining = 30;
        private bool isActualPlayerReady = false;
        private bool isOpponentReady = false;
        private SerializedLine[] opponentDraw;
        private const double MIN_DISTANCE = 1.5;
        private readonly List<Point> drawingPoints = new List<Point>();
        private Polyline lastLineDrawed;
        private bool isPageUnloaded;

        public bool IsChoosingCharacter
        {
            get { return isChoosingCharacter; }
            set
            {
                if (isChoosingCharacter != value)
                {
                    isChoosingCharacter = value;
                    OnPropertyChanged(nameof(IsChoosingCharacter));
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DrawingPage()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new Timer(TimerTick, null, 0, 1000);
        }

        private void TimerTick(object state)
        {   
            secondsRemaining--;

            if (secondsRemaining <= 0)
            {
                Dispatcher.Invoke(AttemptTimeOver);
            }

            Dispatcher.Invoke(() => {
                TbTimer.Text = secondsRemaining.ToString();
            });
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ShowCharacters();

            try
            {
                drawServiceClient = new DrawServiceClient(new InstanceContext(this));
                drawServiceClient.SubscribeToDrawService(gameManager.CurrentMatchCode);
            }
            catch (EndpointNotFoundException)
            {
                StopTimer();
                ServerResponse.ShowServerDownMessage();
                ClearCommunicationChannels();
                RedirectToMainMenu();
            }
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            isPageUnloaded = true;
        }

        private void RedirectToMainMenu()
        {
            MainMenuPage mainMenu = new MainMenuPage();
            NavigationService.Navigate(mainMenu);
        }

        private void ClearCommunicationChannels()
        {
            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        private void AttemptTimeOver()
        {
            StopTimer();
            IsEnabled = false;
            isActualPlayerReady = true;

            drawServiceClient.SendDraw(GetSerializedDraw(), gameManager.CurrentMatchCode);
            CheckBothPlayersReady();
        }

        private void CheckBothPlayersReady()
        {
            if (isActualPlayerReady && isOpponentReady)
            {
                RedirectToAnswerPage();
            }
        }

        private void RedirectToAnswerPage()
        {
            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);

            AnswerPage answerPage = new AnswerPage(opponentDraw);

            gameManager.SubscribePage(answerPage);
            matchStatusManager.SubscribePage(answerPage);

            NavigationService.Navigate(answerPage);

            drawServiceClient.UnsubscribeFromDrawService(gameManager.CurrentMatchCode);
        }

        private void ShowCharacters()
        {
            IcCharacters.ItemsSource = gameManager.CharactersInGame;
        }

        private void CnvsStartDrawing(object sender, MouseButtonEventArgs e)
        {
            isInteractingWithCanvas = true;

            if(isDrawingMode)
            {
                Point currentPoint = e.GetPosition(CnvsDrawing);
                drawingPoints.Add(currentPoint);
            }

            CnvsDrawing.CaptureMouse();
        }

        private void CnvsOnDrawing(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(CnvsDrawing);
            if(IsPointInsideCanvas(currentPoint))
            {
                if (isInteractingWithCanvas)
                {
                    if (isDrawingMode)
                    {
                        drawingPoints.Add(currentPoint);
                        DrawSmoothLine();
                    }
                    else
                    {
                        EraseDrawInCanvas(currentPoint);
                    }
                }
            }
            else
            {
                if (isInteractingWithCanvas)
                {
                    isInteractingWithCanvas = false;
                    DrawSmoothLine();
                    drawingPoints.Clear();
                    lastLineDrawed = null;
                }
            }
        }

        private bool IsPointInsideCanvas(Point currentPoint)
        {
            return currentPoint.X >= 0 && currentPoint.X <= CnvsDrawing.ActualWidth &&
                   currentPoint.Y >= 0 && currentPoint.Y <= CnvsDrawing.ActualHeight;
        }

        private void CnvsEndDrawing(object sender, MouseButtonEventArgs e)
        {
            isInteractingWithCanvas = false;

            Point currentPoint = e.GetPosition(CnvsDrawing);
            if(IsPointInsideCanvas(currentPoint) && isDrawingMode)
            {
                drawingPoints.Add(currentPoint);
                DrawSmoothLine();
            }

            drawingPoints.Clear();
            lastLineDrawed = null;

            CnvsDrawing.ReleaseMouseCapture();
        }

        private void DrawSmoothLine()
        {
            var reducedPoints = DouglasPeuckerReduction(drawingPoints, MIN_DISTANCE);
            var line = new Polyline
            {
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor)),
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeThickness = PEN_THICKNESS
            };

            foreach (var point in reducedPoints)
            {
                line.Points.Add(point);
            }

            CnvsDrawing.Children.Remove(lastLineDrawed);
            CnvsDrawing.Children.Add(line);
            lastLineDrawed = line;
        }

        private List<Point> DouglasPeuckerReduction(List<Point> points, double errorTolerance)
        {
            List<Point> pointsUnderTolerance = points;

            if(points.Count > 3)
            {
                double distanceToFurtherPoint = 0;
                int furthestPointIndex = 0;
                Line lineBetweenEndpoints = new Line
                {
                    X1 = points[0].X,
                    X2 = points[points.Count - 1].X,
                    Y1 = points[0].Y,
                    Y2 = points[points.Count - 1].Y
                };

                for(int i = 1; i < points.Count - 1; i ++)
                {
                    double distanceToPoint = DistancePointToLine(points[i], lineBetweenEndpoints);

                    if(distanceToPoint > distanceToFurtherPoint)
                    {
                        distanceToFurtherPoint = distanceToPoint;
                        furthestPointIndex = i;
                    }
                }

                if(distanceToFurtherPoint >  errorTolerance)
                {
                    List<Point> leftReduction = DouglasPeuckerReduction(points.GetRange(0, furthestPointIndex + 1), MIN_DISTANCE);
                    List<Point> rightReduction = DouglasPeuckerReduction(points.GetRange(furthestPointIndex, points.Count - furthestPointIndex), MIN_DISTANCE);
                
                    pointsUnderTolerance = leftReduction.Concat(rightReduction).ToList();
                }
                else
                {
                    pointsUnderTolerance = new List<Point>{ points[0], points[points.Count - 1] };
                }
            }

            return pointsUnderTolerance;
        }

        private void EraseDrawInCanvas(Point cursorPoint)
        {
            const int ERASER_SIZE = 15;
            var drawPolylines = new List<Polyline>(CnvsDrawing.Children.OfType<Polyline>());

            foreach (var polyline in drawPolylines)
            {
                List<List<Point>> polylineSegments = new List<List<Point>>();
                List<Point> polylinePoints = polyline.Points.ToList();

                List<Point> currentSegment = new List<Point>();
                for (int i = 0; i < polylinePoints.Count; i++) 
                {
                    Point currentPoint = polylinePoints[i];
                    if (DistanceBetweenPoints(currentPoint, cursorPoint) > ERASER_SIZE)
                    {
                        currentSegment.Add(currentPoint);
                    }
                    else
                    {
                        if(currentSegment.Count > 0)
                        {
                            polylineSegments.Add(currentSegment);
                            currentSegment = new List<Point>();
                        }
                    }
                }

                if (currentSegment.Count > 0)
                {
                    polylineSegments.Add(currentSegment);
                }

                UpdatePolyline(polyline, polylineSegments);
            }
        }

        private void UpdatePolyline(Polyline originalPolyline, List<List<Point>> segments)
        {
            CnvsDrawing.Children.Remove(originalPolyline);

            foreach (var segment in segments)
            {
                if(segment.Count > 2)
                {
                    var newPolyline = new Polyline
                    {
                        Stroke = originalPolyline.Stroke,
                        StrokeThickness = originalPolyline.StrokeThickness,
                        StrokeStartLineCap = originalPolyline.StrokeStartLineCap,
                        StrokeEndLineCap = originalPolyline.StrokeEndLineCap
                    };

                    foreach (var point in segment)
                    {
                        newPolyline.Points.Add(point);
                    }

                    CnvsDrawing.Children.Add(newPolyline);
                }
            }
        }

        private double DistanceBetweenPoints(Point pointOne, Point pointTwo)
        {
            double dx = pointOne.X - pointTwo.X;
            double dy = pointOne.Y - pointTwo.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double DistancePointToLine(Point point, Line line)
        {
            double lineStartX = line.X1, lineStartY = line.Y1,
                   lineEndX = line.X2, lineEndY = line.Y2;

            double a = point.X - lineStartX;
            double b = point.Y - lineStartY;
            double c = lineEndX - lineStartX;
            double d = lineEndY - lineStartY;

            double dot = a * c + b * d;
            double lenSq = c * c + d * d;
            double param = dot / lenSq;

            double projectedVectorX, projectedVectorY;
            if (param < 0 || (lineStartX == lineEndX && lineStartY == lineEndY))
            {
                projectedVectorX = lineStartX;
                projectedVectorY = lineStartY;
            }
            else if (param > 1)
            {
                projectedVectorX = lineEndX;
                projectedVectorY = lineEndY;
            }
            else
            {
                projectedVectorX = lineStartX + param * c;
                projectedVectorY = lineStartY + param * d;
            }

            double dx = point.X - projectedVectorX;
            double dy = point.Y - projectedVectorY;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void BtnColorClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string color = button.Tag as string;
                selectedColor = color;
            }

            foreach (Button colorButton in GridColors.Children )
            {
                string colorTag = colorButton.Tag as string;
                if (selectedColor == colorTag)
                {
                    colorButton.BorderBrush = Brushes.Blue;
                }
                else
                {
                    colorButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorTag));
                }
            }
        }

        private void BtnDrawingModeClick(object sender, RoutedEventArgs e)
        {
            isDrawingMode = true;

            BtnDrawingMode.BorderBrush = Brushes.Blue;
            BtnErasingMode.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
        }

        private void BtnEraserModeClick(object sender, RoutedEventArgs e)
        {
            isDrawingMode = false;

            BtnErasingMode.BorderBrush = Brushes.Blue;
            BtnDrawingMode.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
        }

        private void BtnClearDrawingCanvasClick(object sender, RoutedEventArgs e)
        {
            CnvsDrawing.Children.Clear();
        }

        private void BtnExitClick(object sender, RoutedEventArgs e)
        {
            ShowExitConfirmationMessage();
        }

        private void ShowExitConfirmationMessage()
        {
            MessageBoxResult confirmSelection = MessageBox.Show(
                Properties.Resources.msgbConfirmLeaveGameMessage,
                Properties.Resources.msgbConfirmLeaveGameTitle,
                MessageBoxButton.YesNo
            );

            if (!isPageUnloaded)
            {
                if (confirmSelection == MessageBoxResult.Yes)
                {
                    LeaveCurrentGame();

                    drawServiceClient.UnsubscribeFromDrawService(gameManager.CurrentMatchCode);
                    ClearCommunicationChannels();
                    RedirectToMainMenu();
                }
            }
        }

        private void LeaveCurrentGame()
        {
            if (gameManager.IsCurrentMatchHost)
            {
                gameManager.Client.FinishGame(gameManager.CurrentMatchCode);
            }
            else
            {
                gameManager.Client.ExitGame(gameManager.CurrentMatchCode);
            }

            gameManager.RestartRawValues();

            matchStatusManager.Client.StopListeningMatchStatus(matchStatusManager.CurrentMatchCode);
            matchStatusManager.RestartRawValues();
        }

        private void BtnCancelGuessClick(object sender, RoutedEventArgs e)
        {
            IsChoosingCharacter = false;
            BtnGuess.Visibility = Visibility.Visible;
            BtnCancelGuess.Visibility = Visibility.Hidden;
        }

        private void BtnGuessClick(object sender, RoutedEventArgs e)
        {
            IsChoosingCharacter = true;
            BtnCancelGuess.Visibility = Visibility.Visible;
            BtnGuess.Visibility = Visibility.Hidden;
        }

        private void BtnFinishDrawClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirmSelection = MessageBox.Show(
                Properties.Resources.msgbFinishDrawMessage,
                Properties.Resources.msgbFinishDrawTitle,
                MessageBoxButton.YesNo
            );

            if (!isPageUnloaded)
            {
                if (confirmSelection == MessageBoxResult.Yes)
                {
                    StopTimer();
                    isActualPlayerReady = true;
                    drawServiceClient.SendDraw(GetSerializedDraw(), gameManager.CurrentMatchCode);
                    DisableUI();

                    CheckBothPlayersReady();
                }
            }
        }

        private void DisableUI()
        {
            BtnFinishDraw.Opacity = 0.5;
            BtnFinishDraw.IsEnabled = false;

            BtnGuess.Opacity = 0.5;
            BtnGuess.IsEnabled = false;

            CnvsDrawing.IsEnabled = false;
            GridDrawControls.IsEnabled = false;

            BtnFinishDraw.Content = Properties.Resources.lbWaitingOpponent;
        }

        private SerializedLine[] GetSerializedDraw()
        {
            List<SerializedLine> serializedLines = SerializeDraw(CnvsDrawing.Children.OfType<Polyline>().ToList());
            return serializedLines.ToArray();
        }

        private List<SerializedLine> SerializeDraw(List<Polyline> polylines)
        {
            List<SerializedLine> serializedLines = new List<SerializedLine>();

            int totalPoints = 0;
            foreach (var line in polylines)
            {
                List<SerializedPoint> serializedPointsOfLine= new List<SerializedPoint>();
                totalPoints += line.Points.Count;
                foreach(Point point in  line.Points)
                {
                    serializedPointsOfLine.Add(new SerializedPoint
                    {
                        X = point.X,
                        Y = point.Y
                    });
                }

                SerializedLine serializedLine = new SerializedLine
                {
                    Color = ((SolidColorBrush)line.Stroke).Color.ToString(),
                    Points = serializedPointsOfLine.ToArray()
                };

                serializedLines.Add(serializedLine);
            }

            return serializedLines;
        }

        private void GridSelectCharacterClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Character character)
            {
                if(isChoosingCharacter)
                {
                    GuessCharacter(character);
                }
                else
                {
                    ToggleCharacterVisibility(character);
                }
            }
        }

        private void ToggleCharacterVisibility(Character character)
        {
            character.IsSelected = !character.IsSelected;
        }

        private void GuessCharacter(Character character)
        {
            if(!character.IsSelected)
            {
                MessageBoxResult confirmSelection = MessageBox.Show(
                    Properties.Resources.msgbConfirmGuessChoiceMessage,
                    Properties.Resources.msgbConfirmGuessChoiceTitle,
                    MessageBoxButton.YesNo
                );

                if (!isPageUnloaded)
                {
                    if (confirmSelection == MessageBoxResult.Yes)
                    {
                        var isWinner = matchStatusManager.Client.GuessCharacter(character.Name, matchStatusManager.CurrentMatchCode);
                        if (isWinner.StatusCode == ResponseStatus.OK)
                        {
                            RedirectToWinnerPage(isWinner.Value);
                        }
                    }
                }
            }
        }

        private void RedirectToWinnerPage(bool isCurrentPlayerWinner)
        {
            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);

            WinnerPage winnerPage = new WinnerPage();
            if(DataStore.Profile != null)
            {
                winnerPage.InitializeWinnerTextBlock(isCurrentPlayerWinner ? DataStore.Profile.NickName : gameManager.AdversaryNickname);
            }
            else if (gameManager.IsCurrentMatchHost)
            {
                winnerPage.InitializeWinnerTextBlock(isCurrentPlayerWinner ? Properties.Resources.txtHost : gameManager.AdversaryNickname);
            }
            else
            {
                winnerPage.InitializeWinnerTextBlock(isCurrentPlayerWinner ? Properties.Resources.txtGuest : gameManager.AdversaryNickname);
            }

            NavigationService.Navigate(winnerPage);
            StopTimer();

            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode)
        {
            if(matchStatusCode == MatchStatus.GameLost) 
            {
                RedirectToWinnerPage(false);
            }
            else if(matchStatusCode == MatchStatus.GameWon)
            {
                RedirectToWinnerPage(true);
            }
        }

        public void PlayerStatusInMatchChanged(PlayerInMatch player, bool isInMatch)
        {
            NotifyGameHasBeenCanceled();
        }

        private void NotifyGameHasBeenCanceled()
        {
            ClearCommunicationChannels();
            RedirectToMainMenuFromCanceledMatch();
        }

        private void RedirectToMainMenuFromCanceledMatch()
        {
            MainMenuPage mainMenu = new MainMenuPage();
            mainMenu.ShowCanceledMatchMessage();
            NavigationService.Navigate(mainMenu);
        }

        public void DrawReceived(SerializedLine[] adversaryDrawMap)
        {
            opponentDraw = adversaryDrawMap;
            isOpponentReady = true;

            CheckBothPlayersReady();
        }

        private void BorderClueReceivedClick(object sender, MouseButtonEventArgs e)
        {
            BorderClueMessage.Visibility = Visibility.Hidden;
        }

        public void ShowClueSimilarDrawing()
        {
            LbClueDrawing.Content = Properties.Resources.lbWellDone;
            LbClueDrawingMessge.Content = Properties.Resources.lbClueCorrectDraw;

            BorderClueMessage.Visibility = Visibility.Visible;
        }

        public void ShowClueNotSimilarDrawing()
        {
            LbClueDrawing.Content = Properties.Resources.lbAuch;
            LbClueDrawingMessge.Content = Properties.Resources.lbClueWrongDraw;

            BorderClueMessage.Visibility = Visibility.Visible;
        }
    }
}
