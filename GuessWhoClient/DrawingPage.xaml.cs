﻿using GuessWhoClient.Communication;
using GuessWhoClient.GameServices;
using GuessWhoClient.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GuessWhoClient
{
    public partial class DrawingPage : Page, INotifyPropertyChanged, IGamePage, IMatchStatusPage, IDrawServiceCallback
    {
        private bool isInteractingWithCanvas;
        private bool isDrawingMode = true;
        private bool isChoosingCharacter;
        private Point drawStartPoint;
        private string selectedColor = "#000000";
        private const int PEN_THICKNESS = 4;
        public event PropertyChangedEventHandler PropertyChanged;
        private GameManager gameManager = GameManager.Instance;
        private MatchStatusManager matchStatusManager = MatchStatusManager.Instance;
        private DrawServiceClient drawServiceClient;
        private DispatcherTimer timer;
        private int secondsRemaining = 30;
        private bool isActualPlayerReady = false;
        private bool isOpponentReady = false;
        private List<Line> opponentDraw;

        public DrawingPage()
        {
            InitializeComponent();
            InitializeTimer();
        }

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

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ShowCharacters();
            StartTimer();

            drawServiceClient = new DrawServiceClient(new InstanceContext(this));
            drawServiceClient.SubscribeToDrawService(gameManager.CurrentMatchCode);
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
        }

        private void StartTimer()
        {
            timer.Start();
        }

        private void StopTimer()
        {
            timer.Stop();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            secondsRemaining--;

            if (secondsRemaining <= 0)
            {
                AttemptTimeOver();
            }

            TbTimer.Text = secondsRemaining.ToString();
        }

        private void AttemptTimeOver()
        {
            StopTimer();
            this.IsEnabled = false;
            isActualPlayerReady = true;

            drawServiceClient.SendDraw(GetSerializedDraw(), gameManager.CurrentMatchCode);
        }

        private void RedirectToAnswerPage(List<Line> opponentDraw)
        {
            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);
            drawServiceClient.UnsubscribeFromDrawService(gameManager.CurrentMatchCode);

            AnswerPage answerPage = new AnswerPage();
            answerPage.PaintOpponentDraw(opponentDraw);
            this.NavigationService.Navigate(answerPage);

            gameManager.SubscribePage(answerPage);
            matchStatusManager.SubscribePage(answerPage);
        }

        private void ShowCharacters()
        {
            IcCharacters.ItemsSource = RecoverChatacters();
        }

        private List<Character> RecoverChatacters()
        {
            List<Character> charactersList = new List<Character>();

            string PROJECT_DIRECTORY = System.IO.Path.Combine(AppContext.BaseDirectory, "..\\..\\");
            string CHARACTERS_FOLDER = System.IO.Path.Combine(PROJECT_DIRECTORY, "Resources\\Characters");
            string[] imageFiles = Directory.GetFiles(CHARACTERS_FOLDER, "*.png");

            foreach (string imagePath in imageFiles)
            {
                Character character = new Character
                {
                    IsSelected = false,
                    Avatar = new BitmapImage(new Uri(imagePath, UriKind.Relative))
                };
                charactersList.Add(character);
            }

            return charactersList;
        }

        private void CnvsStartDrawing(object sender, MouseButtonEventArgs e)
        {
            isInteractingWithCanvas = true;
            drawStartPoint = e.GetPosition(CnvsDrawing);

            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor));
            var line = new Line
            {
                X1 = drawStartPoint.X,
                Y1 = drawStartPoint.Y,
                X2 = drawStartPoint.X + 1,
                Y2 = drawStartPoint.Y + 1,
                Stroke = brush,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeThickness = PEN_THICKNESS
            };

            CnvsDrawing.Children.Add(line);
        }

        private void CnvsOnDrawing(object sender, MouseEventArgs e)
        {
            var currentPoint = e.GetPosition(CnvsDrawing);
            if(isInteractingWithCanvas)
            {
                if (isDrawingMode)
                {
                    DrawInCanvas(currentPoint);
                }
                else
                {
                    EraseDrawInCanvas(currentPoint);
                }
            }
        }

        private void DrawInCanvas(Point point)
        {
            var line = new Line
            {
                X1 = drawStartPoint.X,
                Y1 = drawStartPoint.Y,
                X2 = point.X,
                Y2 = point.Y,
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(selectedColor)),
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeThickness = PEN_THICKNESS
            };

            CnvsDrawing.Children.Add(line);
            drawStartPoint = point;
        }

        private void EraseDrawInCanvas(Point point)
        {
            List<Line> linesToRemove = new List<Line>();
            const int ERASER_SIZE = 15;

            foreach (var line in CnvsDrawing.Children.OfType<Line>())
            {
                double distanceToLine = DistancePointToLine(point, line);
                if (distanceToLine < ERASER_SIZE)
                {
                    linesToRemove.Add(line);
                }
            }

            foreach (var lineToRemove in linesToRemove)
            {
                CnvsDrawing.Children.Remove(lineToRemove);
            }
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

        private void CnvsEndDrawing(object sender, MouseButtonEventArgs e)
        {
            isInteractingWithCanvas = false;
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

            if (confirmSelection == MessageBoxResult.Yes)
            {
                LeaveCurrentGame();
                RedirectToMainMenu();
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

        private void RedirectToMainMenu()
        {
            gameManager.UnsubscribePage(this);
            matchStatusManager.UnsubscribePage(this);
            drawServiceClient.UnsubscribeFromDrawService(gameManager.CurrentMatchCode);

            MainMenuPage mainMenu = new MainMenuPage();
            this.NavigationService.Navigate(mainMenu);
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

            if (confirmSelection == MessageBoxResult.Yes)
            {
                isActualPlayerReady = true;
                drawServiceClient.SendDraw(GetSerializedDraw(), gameManager.CurrentMatchCode);
                DisableUI();

                CheckBothPlayersReady();
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
            List<SerializedLine> serializedLines = SerializeDraw(CnvsDrawing.Children.OfType<Line>());
            return serializedLines.ToArray();
        }

        private List<SerializedLine> SerializeDraw(IEnumerable<Line> lines)
        {
            List<SerializedLine> serializedLines = new List<SerializedLine>();

            foreach (var line in lines)
            {
                SerializedLine serializedLine = new SerializedLine
                {
                    Color = ((SolidColorBrush)line.Stroke).Color.ToString(),
                    StartPoint = new Point(line.X1, line.Y1),
                    EndPoint = new Point(line.X2, line.Y2)
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

                if(confirmSelection == MessageBoxResult.Yes)
                {
                    
                }
            }
        }

        public void MatchStatusChanged(MatchStatus matchStatusCode) //Only Game Won / Game Lost
        {
            throw new NotImplementedException();
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
            this.NavigationService.Navigate(mainMenu);
        }

        private void ClearCommunicationChannels()
        {
            gameManager.RestartRawValues();
            matchStatusManager.RestartRawValues();
        }

        public void DrawReceived(SerializedLine[] adversaryDrawMap)
        {
            opponentDraw = UnserializeDraw(adversaryDrawMap);

            isOpponentReady = true;
            CheckBothPlayersReady();
        }

        private void CheckBothPlayersReady()
        {
            Console.WriteLine(isActualPlayerReady + " " + isOpponentReady);

            if (isActualPlayerReady && isOpponentReady)
            {
                RedirectToAnswerPage(opponentDraw);
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
    }
}
