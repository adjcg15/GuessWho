using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GuessWhoClient
{
    public partial class DrawingPage : Page
    {
        private bool isInteractingWithCanvas;
        private bool isDrawingMode = true;
        private Point drawStartPoint;
        private string selectedColor = "#000000";
        private const int PEN_THICKNESS = 4;

        public DrawingPage()
        {
            InitializeComponent();
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
                StrokeThickness = PEN_THICKNESS
            };

            CnvsDrawing.Children.Add(line);

            drawStartPoint = point;
        }

        private void EraseDrawInCanvas(Point point)
        {
            var linesToRemove = new List<Line>();

            foreach (var element in CnvsDrawing.Children.OfType<Line>())
            {
                var distanceToLine = DistancePointToLine(point, new Point(element.X1, element.Y1), new Point(element.X2, element.Y2));

                if (distanceToLine < 10)
                {
                    linesToRemove.Add(element);
                }
            }

            foreach (var lineToRemove in linesToRemove)
            {
                CnvsDrawing.Children.Remove(lineToRemove);
            }
        }

        private double DistancePointToLine(Point point, Point lineStart, Point lineEnd)
        {
            double a = point.X - lineStart.X;
            double b = point.Y - lineStart.Y;
            double c = lineEnd.X - lineStart.X;
            double d = lineEnd.Y - lineStart.Y;

            double dot = a * c + b * d;
            double lenSq = c * c + d * d;
            double param = dot / lenSq;

            double xx, yy;

            if (param < 0 || (lineStart.X == lineEnd.X && lineStart.Y == lineEnd.Y))
            {
                xx = lineStart.X;
                yy = lineStart.Y;
            }
            else if (param > 1)
            {
                xx = lineEnd.X;
                yy = lineEnd.Y;
            }
            else
            {
                xx = lineStart.X + param * c;
                yy = lineStart.Y + param * d;
            }

            double dx = point.X - xx;
            double dy = point.Y - yy;

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

            foreach (Button otherButton in GridColors.Children )
            {
                string otherColorTag = otherButton.Tag as string;

                if (selectedColor == otherColorTag)
                {
                    otherButton.BorderBrush = Brushes.Blue;
                }
                else
                {
                    otherButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(otherColorTag));
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

        private void BtnGuessClick(object sender, RoutedEventArgs e)
        {

        }

        private void BtnFinishClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
