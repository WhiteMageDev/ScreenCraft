using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenCraft
{
    public class SelectedVisual
    {
        private Rectangle borderRect;
        private Canvas canvasGroup;
        private List<Rectangle> corners;
        private int cornerSize = 8;
        public SelectedVisual(Canvas mainCanvas)
        {
            canvasGroup = new Canvas();
            borderRect = new Rectangle()
            {
                Width = 0,
                Height = 0,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = 1,

            };
            canvasGroup.Children.Add(borderRect);
            corners = new List<Rectangle>();

            for (int i = 0; i < 8; i++)
            {
                Rectangle corner = new Rectangle()
                {
                    Width = cornerSize,
                    Height = cornerSize,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.2,
                    Fill = Brushes.Black,
                };
                corners.Add(corner);
                canvasGroup.Children.Add(corner);
            }

            mainCanvas.Children.Add(canvasGroup);

        }
        public void UpdatePosition(Rectangle selectedArea)
        {
            Canvas.SetLeft(canvasGroup, Canvas.GetLeft(selectedArea));
            Canvas.SetTop(canvasGroup, Canvas.GetTop(selectedArea));
            canvasGroup.Width = selectedArea.Width;
            canvasGroup.Height = selectedArea.Height;

            Canvas.SetLeft(borderRect, 0);
            Canvas.SetTop(borderRect, 0);
            borderRect.Width = canvasGroup.Width;
            borderRect.Height = canvasGroup.Height;

            double width = canvasGroup.Width;
            double height = canvasGroup.Height;

            List<Point> points = new List<Point>()
            {
                new Point(0,0),
                new Point(width / 2, 0 ),
                new Point(width, 0),
                new Point(0, height / 2),
                new Point(width, height / 2),
                new Point(0, height),
                new Point(width / 2, height),
                new Point(width, height),
            };

            for (int i = 0; i < 8; i++)
            {
                Point n = new Point(points[i].X - cornerSize / 2, points[i].Y - cornerSize / 2);
                Canvas.SetLeft(corners[i], n.X);
                Canvas.SetTop(corners[i], n.Y);
            }
            Panel.SetZIndex(canvasGroup, 198);
        }

        public void Hide()
        {
            canvasGroup.Visibility = Visibility.Collapsed;
        }
        public void Show()
        {
            canvasGroup.Visibility = Visibility.Visible;
        }
    }
}
