using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GuessWhoClient
{
    public partial class DrawingPage : Page, INotifyPropertyChanged
    {
        private bool isInteractingWithCanvas;
        private bool isDrawingMode = true;
        private bool isChoosingCharacter;
        private Point drawStartPoint;
        private string selectedColor = "#000000";
        private const int PEN_THICKNESS = 4;
        public event PropertyChangedEventHandler PropertyChanged;

        public DrawingPage()
        {
            InitializeComponent();
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
    }
}
