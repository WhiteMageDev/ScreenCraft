﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace ScreenCraft
{
    public class EditorContext
    {
        private IEditorState currentState;

        public void SetState(IEditorState state)
        {
            currentState = state;
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            currentState.HandleMouseDown(sender, e);
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            currentState.HandleMouseMove(sender, e);
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            currentState.HandleMouseUp(sender, e);
        }
    }

    public class FreePenDrawingState : IEditorState
    {
        private readonly Canvas canvas;
        private Point startPoint;
        private Color color;
        private Polyline? currentPolyLine;
        private bool markerCheck;

        public FreePenDrawingState(Canvas canvas)
        {
            this.canvas = canvas;
            markerCheck = false;
        }
        public void MarkerCheck(bool val)
        {
            markerCheck = val;
        }
        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            color = EditorWindow.SelectedColor;
            if (markerCheck)
                currentPolyLine = new Polyline
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 5 + EditorWindow.GlobalSize * 15,
                    Opacity = 0.5,
                    Points = new PointCollection { startPoint }
                };
            else
                currentPolyLine = new Polyline
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1 + EditorWindow.GlobalSize * 9,
                    Points = new PointCollection { startPoint }
                };
            canvas.Children.Add(currentPolyLine);
        }
        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && currentPolyLine != null)
            {
                Point currentPoint = e.GetPosition(canvas);
                currentPolyLine.Points.Add(currentPoint);
            }
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentPolyLine != null)
            {
                EditorWindow.ObjectsListAdd(currentPolyLine);
                currentPolyLine.Points = SmoothPolyline(currentPolyLine.Points, 2);
            }
            currentPolyLine = null;
        }
        private PointCollection SmoothPolyline(PointCollection points, int smoothingFactor)
        {
            PointCollection smoothedPoints = new PointCollection();
            for (int i = 0; i < points.Count; i++)
            {
                int startIndex = Math.Max(0, i - smoothingFactor);
                int endIndex = Math.Min(points.Count - 1, i + smoothingFactor);
                double sumX = 0;
                double sumY = 0;
                int count = 0;
                for (int j = startIndex; j <= endIndex; j++)
                {
                    sumX += points[j].X;
                    sumY += points[j].Y;
                    count++;
                }
                double newX = sumX / count;
                double newY = sumY / count;
                smoothedPoints.Add(new Point(newX, newY));
            }
            return smoothedPoints;
        }
    }
    public class LineDrawingState : IEditorState
    {
        private readonly Canvas canvas;
        private Point startPoint;
        private Color color;
        private Line? currentLine;

        public LineDrawingState(Canvas canvas)
        {
            this.canvas = canvas;
        }
        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            color = EditorWindow.SelectedColor;
            currentLine = new Line
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1 + EditorWindow.GlobalSize * 9,
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = startPoint.X,
                Y2 = startPoint.Y
            };
            canvas.Children.Add(currentLine);
        }
        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && currentLine != null)
            {
                Point currentPoint = e.GetPosition(canvas);
                currentLine.X2 = currentPoint.X;
                currentLine.Y2 = currentPoint.Y;
            }
        }
        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentLine != null)
                EditorWindow.ObjectsListAdd(currentLine);
            currentLine = null;
        }
    }
    public class ArrowDrawingState : IEditorState
    {
        private readonly Canvas canvas;
        private Point startPoint;
        private Color color;
        private Line? currentLine;
        private Polygon? arrowhead;
        public ArrowDrawingState(Canvas canvas)
        {
            this.canvas = canvas;
        }
        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            color = EditorWindow.SelectedColor;
            startPoint = e.GetPosition(canvas);

            currentLine = new Line
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1 + EditorWindow.GlobalSize * 9,
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = startPoint.X,
                Y2 = startPoint.Y
            };
            canvas.Children.Add(currentLine);

            Point arrowPoint1 = new Point(startPoint.X + 10, startPoint.Y + 10);
            Point arrowPoint2 = new Point(startPoint.X - 10, startPoint.Y + 10);

            arrowhead = new Polygon();
            arrowhead.Stroke = new SolidColorBrush(color);
            arrowhead.Fill = new SolidColorBrush(color);
            arrowhead.Points = new PointCollection { startPoint, arrowPoint1, arrowPoint2 };

            canvas.Children.Add(arrowhead);
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (currentLine != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(canvas);

                currentLine.X2 = currentPoint.X;
                currentLine.Y2 = currentPoint.Y;

                if (arrowhead != null)
                {
                    double deltaX = currentPoint.X - startPoint.X;
                    double deltaY = currentPoint.Y - startPoint.Y;
                    double len = 5 + EditorWindow.GlobalSize * 9;
                    double side = len / 1.5;

                    double angle = Math.Atan2(deltaY, deltaX);

                    Point arrowPoint0 = new Point(currentPoint.X - len * Math.Cos(angle), currentPoint.Y - len * Math.Sin(angle));

                    Point arrowPoint1 = new Point(arrowPoint0.X - side * deltaY / Math.Sqrt(deltaX * deltaX + deltaY * deltaY),
                                                  arrowPoint0.Y + side * deltaX / Math.Sqrt(deltaX * deltaX + deltaY * deltaY));

                    Point arrowPoint2 = new Point(arrowPoint0.X + side * deltaY / Math.Sqrt(deltaX * deltaX + deltaY * deltaY),
                                                  arrowPoint0.Y - side * deltaX / Math.Sqrt(deltaX * deltaX + deltaY * deltaY));

                    arrowhead.Points[1] = arrowPoint1;
                    arrowhead.Points[2] = arrowPoint2;
                    arrowhead.Points[0] = new Point(currentPoint.X + len * Math.Cos(angle), currentPoint.Y + len * Math.Sin(angle));
                }
            }
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (currentLine != null && arrowhead != null)
            {

                EditorWindow.ObjectsListAdd(currentLine);
                EditorWindow.ObjectsListAdd(arrowhead);
            }
            currentLine = null;
            arrowhead = null;
        }
    }
    public class RectangleDrawingState : IEditorState
    {
        private readonly Canvas canvas;
        private EditorWindow editor;
        private Rectangle? rectangle;
        private Point startPoint;
        private Color color;
        private bool blurCheck;
        private bool fillCheck;

        public RectangleDrawingState(Canvas canvas, EditorWindow editor)
        {
            this.editor = editor;
            this.canvas = canvas;
            blurCheck = false;
            fillCheck = false;
        }
        public void BlurCheck()
        {
            blurCheck = true;
            fillCheck = false;
        }
        public void FillCheck()
        {
            blurCheck = false;
            fillCheck = true;
        }
        public void NofillCheck()
        {
            blurCheck = false;
            fillCheck = false;
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            color = EditorWindow.SelectedColor;
            startPoint = e.GetPosition(canvas);

            if (!blurCheck && !fillCheck)
            {
                // regular rect no fill no blur
                rectangle = new Rectangle
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1 + EditorWindow.GlobalSize * 9,
                    Fill = Brushes.Transparent,
                    Width = 0,
                    Height = 0
                };
            }
            else if (blurCheck)
            {
                // blure rect
                rectangle = new Rectangle
                {
                    Stroke = Brushes.Transparent,
                    StrokeDashArray = new DoubleCollection(new double[] { 4, 2 }),
                    StrokeThickness = 1,
                    Fill = new SolidColorBrush(Color.FromArgb(64, 255, 0, 0)),
                Width = 0,
                    Height = 0
                };
            }
            else if (fillCheck)
            {
                // filled rect
                rectangle = new Rectangle
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = 1 + EditorWindow.GlobalSize * 9,
                    Fill = new SolidColorBrush(color),
                    Width = 0,
                    Height = 0
                };
            }
            Canvas.SetLeft(rectangle, startPoint.X);
            Canvas.SetTop(rectangle, startPoint.Y);

            canvas.Children.Add(rectangle);
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (!canvas.IsMouseOver)
            {
                return;
            }
            if (rectangle != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(canvas);
                float width = (float)(currentPoint.X - startPoint.X);
                float height = (float)(currentPoint.Y - startPoint.Y);

                rectangle.Width = MathF.Abs(width);
                rectangle.Height = MathF.Abs(height);

                if (width > 0)
                    Canvas.SetLeft(rectangle, startPoint.X);
                else
                    Canvas.SetLeft(rectangle, currentPoint.X);

                if (height > 0)
                    Canvas.SetTop(rectangle, startPoint.Y);
                else
                    Canvas.SetTop(rectangle, currentPoint.Y);
            }
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (rectangle != null)
            {
                if (blurCheck)
                {
                    double left = Canvas.GetLeft(rectangle);
                    double top = Canvas.GetTop(rectangle);
                    double width = rectangle.Width;
                    double height = rectangle.Height;

                    canvas.Children.Remove(rectangle);
                    if (left <= 0 || top <= 0 || width <= 0 || height <= 0) return;

                    editor.SelectedArea.Visibility = Visibility.Hidden;
                    editor.SelectedVisual.Hide();
                    editor.HideRectMenu();

                    Image blurredImage = BlurSelectedArea(left, top, width, height);
                    canvas.Children.Add(blurredImage);
                    EditorWindow.ObjectsListAdd(blurredImage);

                    editor.SelectedArea.Visibility = Visibility.Visible;
                    editor.SelectedVisual.Show();
                    editor.ShowRectMenu();
                }
                EditorWindow.ObjectsListAdd(rectangle);
                rectangle = null;
            }
        }

        private Image BlurSelectedArea(double l, double t, double w, double h)
        {
            double left = l;
            double top = t;
            double width = w;
            double height = h;

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Default);
            rtb.Render(canvas);

            CroppedBitmap croppedBitmap = new CroppedBitmap(rtb, new Int32Rect((int)left, (int)top, (int)width, (int)height));
            BlurEffect blurEffect = new BlurEffect { Radius = 10 };
            Image blurredImage = new Image
            {
                Source = croppedBitmap,
                Effect = blurEffect
            };
            Canvas.SetLeft(blurredImage, left);
            Canvas.SetTop(blurredImage, top);
            return blurredImage;
        }
    }
    public class TextDrawingState : IEditorState
    {
        private readonly Canvas canvas;
        private TextBox? textBox;
        private Point lastPoint;
        private Color color;

        AdornerDecorator adornerDecorator;

        public TextDrawingState(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            color = EditorWindow.SelectedColor;
            Point point = e.GetPosition(canvas);
            if (textBox == null)
            {
                lastPoint = point;
                textBox = new TextBox
                {
                    Foreground = new SolidColorBrush(color),
                    BorderThickness = new Thickness(0),
                    FontSize = 8 + EditorWindow.GlobalSize * 24,
                    Width = double.NaN,
                    Height = double.NaN,
                    AcceptsReturn = true,
                    Background = Brushes.Transparent,
                    TextWrapping = TextWrapping.Wrap,
                    Visibility = Visibility.Visible
                };
                adornerDecorator = new AdornerDecorator();
                adornerDecorator.Child = textBox;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(textBox);
                adornerLayer.Add(new DottedBorderAdorner(textBox));


                Canvas.SetLeft(adornerDecorator, point.X);
                Canvas.SetTop(adornerDecorator, point.Y);
                canvas.Children.Add(adornerDecorator);

                textBox.Visibility = Visibility.Visible;
                textBox.Focus();
            }
            else
            {
                TextBlock textBlock = new TextBlock
                {
                    FontSize = 8 + EditorWindow.GlobalSize * 24,
                    Foreground = new SolidColorBrush(color)
                };

                Canvas.SetLeft(textBlock, lastPoint.X);
                Canvas.SetTop(textBlock, lastPoint.Y);

                canvas.Children.Add(textBlock);
                textBlock.Text = textBox.Text;
                canvas.Children.Remove(adornerDecorator);
                EditorWindow.ObjectsListAdd(textBlock);
                textBox = null;
            }
        }

        public void HandleMouseMove(object sender, MouseEventArgs e) { }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e) { }
    }
    public class ScreenshotAreaPickingState : IEditorState
    {

        private EditorWindow editor;
        private Point startPoint;

        private Label? rectSizeLabel;

        public ScreenshotAreaPickingState(EditorWindow editor)
        {
            this.editor = editor;
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            Trace.WriteLine("Canvas_MouseLeftButtonDown");
            editor.HideRectMenu();
            editor.rectDrawingCheck = true;

            if (editor.SelectedArea != null)
                ClearRect();

            startPoint = e.GetPosition(editor.MainCanvas);


            editor.SelectedArea = new Rectangle
            {
                Stroke = Brushes.White,
                StrokeDashArray = new DoubleCollection(new double[] { 2, 2 }),
                StrokeThickness = 1,
                Fill = Brushes.Transparent,
                Width = 0,
                Height = 0
                
            };
            Panel.SetZIndex(editor.SelectedArea, 199);
            Canvas.SetLeft(editor.SelectedArea, startPoint.X);
            Canvas.SetTop(editor.SelectedArea, startPoint.Y);

            editor.MainCanvas.Children.Add(editor.SelectedArea);
        }
        public void HandleMouseMove(object sender, MouseEventArgs e)
        {

            if (editor.SelectedArea == null) return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(editor.MainCanvas);

                double width = currentPoint.X - startPoint.X;
                double height = currentPoint.Y - startPoint.Y;

                editor.SelectedArea.Width = (int)Math.Round(Math.Abs(width));
                editor.SelectedArea.Height = (int)Math.Round(Math.Abs(height));

                int startX = (int)Math.Round(startPoint.X);
                int startY = (int)Math.Round(startPoint.Y);
                int currentX = (int)Math.Round(currentPoint.X);
                int currentY = (int)Math.Round(currentPoint.Y);

                Canvas.SetLeft(editor.SelectedArea, width > 0 ? startX : currentX);
                Canvas.SetTop(editor.SelectedArea, height > 0 ? startY : currentY);

                UpdateRectangleLabel(editor.SelectedArea.Width, editor.SelectedArea.Height);
                editor.SelectedVisual.UpdatePosition(editor.SelectedArea);
            }
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (editor.SelectedArea != null)
            {
                editor.rectDrawingCheck = false;


                editor.MainCanvas.Children.Remove(rectSizeLabel);
                rectSizeLabel = null;
                editor.UpdateImage();
                editor.ShowRectMenu();
            }
        }


        private void ClearRect()
        {
            Trace.WriteLine("ClearRect");

            editor.MainCanvas.Children.Remove(editor.SelectedArea);
            editor.MainImage.Clip = new RectangleGeometry(new Rect(0, 0, 0, 0));
            editor.SelectedArea = null;
        }
        private void UpdateRectangleLabel(double width, double height)
        {
            if (rectSizeLabel == null)
            {
                rectSizeLabel = new Label
                {
                    Content = $"{width}x{height}",
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Background = Brushes.Black
                };
                editor.MainCanvas.Children.Add(rectSizeLabel);
            }
            else
            {
                rectSizeLabel.Content = $"{width}x{height}";
            }
            double rectTop = Canvas.GetTop(editor.SelectedArea) + 5;
            double rectLeft = Canvas.GetLeft(editor.SelectedArea) + 5;
            bool w = rectTop - rectSizeLabel.ActualHeight < 0;
            bool h = rectLeft + rectSizeLabel.ActualWidth < editor.MainCanvas.ActualWidth;

            Canvas.SetTop(rectSizeLabel, w ? rectTop : rectTop - rectSizeLabel.ActualHeight - 10);
            Canvas.SetLeft(rectSizeLabel, h ? rectLeft : rectLeft - rectSizeLabel.ActualWidth - 10);
        }
    }
    public class ScreenshotAreaMovingState : IEditorState
    {
        private EditorWindow editor;
        private Canvas MainCanvas;
        private Point startPoint;
        private bool isResizing = false;
        private const double cornerSize = 8;
        private DateTime lastClickTime = DateTime.MinValue;
        private const int doubleClickTimeThreshold = 500;
        private Corner currentCorner;
        private enum Corner
        {
            TopLeft,
            TopMiddle,
            TopRight,
            BottomLeft,
            BottomMiddle,
            BottomRight,
            MiddleLeft,
            MiddleRight,
            None
        }
        public ScreenshotAreaMovingState(EditorWindow editor)
        {
            this.editor = editor;
            MainCanvas = editor.MainCanvas;
        }
        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (editor.SelectedArea == null) return;

            DateTime now = DateTime.Now;
            if ((now - lastClickTime).TotalMilliseconds <= doubleClickTimeThreshold)
            {
                lastClickTime = DateTime.MinValue;
                HandleDoubleClick();
            }
            else
                lastClickTime = now;

            editor.HideRectMenu();
            startPoint = Mouse.GetPosition(MainCanvas);

            currentCorner = GetCorner(startPoint);
            isResizing = true;

            editor.SelectedArea.CaptureMouse();
        }
        private void SetCursour(Corner corner)
        {
            Cursor pointer = Cursors.Arrow;
            switch (corner)
            {
                case Corner.TopLeft:
                case Corner.BottomRight:
                    pointer = Cursors.SizeNWSE;
                    break;
                case Corner.TopMiddle:
                case Corner.BottomMiddle:
                    pointer = Cursors.SizeNS;
                    break;
                case Corner.TopRight:
                case Corner.BottomLeft:
                    pointer = Cursors.SizeNESW;
                    break;
                case Corner.MiddleLeft:
                case Corner.MiddleRight:
                    pointer = Cursors.SizeWE;
                    break;
                case Corner.None:
                    pointer = Cursors.SizeAll;
                    break;
            }
            editor.Cursor = pointer;
        }
        private Corner GetCorner(Point position)
        {
            Rectangle rect = editor.SelectedArea;

            double left = Canvas.GetLeft(rect);
            double top = Canvas.GetTop(rect);
            double width = rect.Width;
            double height = rect.Height;

            if(position.X - cornerSize < left)
            {
                if(position.Y - cornerSize < top)
                {
                    return Corner.TopLeft;
                }
                if (position.Y + cornerSize > top + height)
                {
                    return Corner.BottomLeft;
                }
                if (position.Y - cornerSize < top + height / 2 && position.Y + cornerSize > top + height / 2)
                {
                    return Corner.MiddleLeft;
                }

            }
            else if(position.X + cornerSize > left + width)
            {
                if (position.Y - cornerSize < top)
                {
                    return Corner.TopRight;
                }
                if (position.Y + cornerSize > top + height)
                {
                    return Corner.BottomRight;
                }
                if (position.Y - cornerSize < top + height / 2 && position.Y + cornerSize > top + height / 2)
                {
                    return Corner.MiddleRight;
                }
            }
            else if(position.X - cornerSize < left + width / 2 && position.X + cornerSize > left + width / 2)
            {
                if (position.Y - cornerSize < top)
                {
                    return Corner.TopMiddle;
                }
                if (position.Y + cornerSize > top + height)
                {
                    return Corner.BottomMiddle;
                }
            }
            return Corner.None;
        }
        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (editor.SelectedArea == null) return;

            Point newPoint = Mouse.GetPosition(MainCanvas);

            if (isResizing)
            {
                SetCursour(currentCorner);

                double left = Canvas.GetLeft(editor.SelectedArea);
                double top = Canvas.GetTop(editor.SelectedArea);
                double width = editor.SelectedArea.Width;
                double height = editor.SelectedArea.Height;
                double diffX = (newPoint.X - startPoint.X);
                double diffY = (newPoint.Y - startPoint.Y);

                if (currentCorner != Corner.None)
                {
                    switch (currentCorner)
                    {
                        case Corner.MiddleRight:
                            double newWidthMR = width + diffX;
                            if (newWidthMR < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                currentCorner = Corner.MiddleLeft;
                            }
                            else
                            {
                                editor.SelectedArea.Width = Math.Abs(newPoint.X - left);
                            }
                            break;

                        case Corner.MiddleLeft:
                            double newWidthML = width - diffX;
                            if (newWidthML < 0)
                            {
                                currentCorner = Corner.MiddleRight;
                                editor.SelectedArea.Width = 0;
                                Canvas.SetLeft(editor.SelectedArea, left + width);
                            }
                            else
                            {
                                Canvas.SetLeft(editor.SelectedArea, newPoint.X);
                                editor.SelectedArea.Width = Math.Abs(width + (left - newPoint.X));
                            }
                            break;

                        case Corner.BottomMiddle:
                            double newHeightBM = height + diffY;
                            if (newHeightBM < 0)
                            {
                                editor.SelectedArea.Height = 0;
                                currentCorner = Corner.TopMiddle;
                            }
                            else
                            {
                                editor.SelectedArea.Height = Math.Abs(newPoint.Y - top);
                            }
                            break;

                        case Corner.TopMiddle:
                            double newHeightTM = height - diffY;
                            if (newHeightTM < 0)
                            {
                                currentCorner = Corner.BottomMiddle;
                                editor.SelectedArea.Height = 0;
                                Canvas.SetTop(editor.SelectedArea, top + height);
                            }
                            else
                            {
                                Canvas.SetTop(editor.SelectedArea, newPoint.Y);
                                editor.SelectedArea.Height = Math.Abs(height + (top - newPoint.Y));
                            }
                            break;

                        case Corner.TopRight:

                            double newWidthTR = width + diffX;
                            double newHeightTR = height - diffY;

                            if (newWidthTR < 0 && newHeightTR < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                editor.SelectedArea.Height = 0;
                                Canvas.SetTop(editor.SelectedArea, top + height);
                                currentCorner = Corner.BottomLeft;
                            }
                            else if (newWidthTR < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                currentCorner = Corner.TopLeft;
                            }
                            else if (newHeightTR < 0)
                            {
                                editor.SelectedArea.Height = 0;
                                Canvas.SetTop(editor.SelectedArea, top + height);
                                currentCorner = Corner.BottomRight;
                            }
                            else
                            {
                                Canvas.SetTop(editor.SelectedArea, newPoint.Y);
                                editor.SelectedArea.Height = Math.Abs(height + (top - newPoint.Y));
                                editor.SelectedArea.Width = Math.Abs(newPoint.X - left);
                            }
                            break;

                        case Corner.TopLeft:

                            double newWidthTL = width - diffX;
                            double newHeightTL = height - diffY;

                            if (newWidthTL < 0 && newHeightTL < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                editor.SelectedArea.Height = 0;
                                Canvas.SetTop(editor.SelectedArea, top + height);
                                Canvas.SetLeft(editor.SelectedArea, left + width);
                                currentCorner = Corner.BottomRight;
                            }
                            else if (newWidthTL < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                currentCorner = Corner.TopRight;
                            }
                            else if (newHeightTL < 0)
                            {
                                editor.SelectedArea.Height = 0;
                                Canvas.SetTop(editor.SelectedArea, top + height);
                                currentCorner = Corner.BottomLeft;
                            }
                            else
                            {
                                Canvas.SetLeft(editor.SelectedArea, newPoint.X);
                                Canvas.SetTop(editor.SelectedArea, newPoint.Y);
                                editor.SelectedArea.Width = Math.Abs(width + (left - newPoint.X));
                                editor.SelectedArea.Height = Math.Abs(height + (top - newPoint.Y));
                            }
                            break;

                        case Corner.BottomLeft:

                            double newWidthBL = width - diffX;
                            double newHeightBL = height + diffY;

                            if (newWidthBL < 0 && newHeightBL < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                editor.SelectedArea.Height = 0;
                                Canvas.SetLeft(editor.SelectedArea, left + width);
                                currentCorner = Corner.TopRight;
                            }
                            else if (newWidthBL < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                Canvas.SetLeft(editor.SelectedArea, left + width);
                                currentCorner = Corner.BottomRight;
                            }
                            else if (newHeightBL < 0)
                            {
                                editor.SelectedArea.Height = 0;
                                currentCorner = Corner.TopLeft;
                            }
                            else
                            {
                                Canvas.SetLeft(editor.SelectedArea, newPoint.X);
                                editor.SelectedArea.Width = Math.Abs(width + (left - newPoint.X));
                                editor.SelectedArea.Height = Math.Abs(newPoint.Y - top);
                            }
                            break;

                        case Corner.BottomRight:

                            double newWidthBR = width + diffX;
                            double newHeightBR = height + diffY;

                            if (newWidthBR < 0 && newHeightBR < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                editor.SelectedArea.Height = 0;
                                currentCorner = Corner.TopLeft;
                            }
                            else if (newWidthBR < 0)
                            {
                                editor.SelectedArea.Width = 0;
                                currentCorner = Corner.BottomLeft;
                            }
                            else if (newHeightBR < 0)
                            {
                                editor.SelectedArea.Height = 0;
                                currentCorner = Corner.TopRight;
                            }
                            else
                            {
                                editor.SelectedArea.Width = Math.Abs(newPoint.X - left);
                                editor.SelectedArea.Height = Math.Abs(newPoint.Y - top);
                            }
                            break;
                    }
                }
                else if (currentCorner == Corner.None)
                {
                    double newLeft = left + diffX;
                    double newtop = top + diffY;

                    if (left + diffX < 0)
                    {
                        newLeft = 0;
                        width += diffX;
                    }
                    else if (left + width + diffX > editor.MainCanvas.ActualWidth)
                    {
                        width -= diffX;
                    }
                    if (top + diffY < 0)
                    {
                        newtop = 0;
                        height += diffY;
                    }
                    else if (top + height + diffY > editor.MainCanvas.ActualHeight)
                    {
                        height -= diffY;
                    }
                    if (width > 5 && height > 5)
                    {
                        Canvas.SetLeft(editor.SelectedArea, newLeft);
                        Canvas.SetTop(editor.SelectedArea, newtop);
                        editor.SelectedArea.Width = Math.Abs(width);
                        editor.SelectedArea.Height = Math.Abs(height);
                    }
                }
                startPoint = newPoint;
                editor.UpdateImage();
                editor.SelectedVisual.UpdatePosition(editor.SelectedArea);
            }
            else
            {
                SetCursour(GetCorner(newPoint));
            }
        }
        public void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (editor.SelectedArea == null) return;
            editor.ShowRectMenu();
            isResizing = false;
            editor.SelectedArea.ReleaseMouseCapture();
        }
        private void HandleDoubleClick()
        {
            Trace.WriteLine("HandleDoubleClick");
            Canvas.SetLeft(editor.SelectedArea, 0);
            Canvas.SetTop(editor.SelectedArea, 0);
            editor.SelectedArea.Width = editor.MainCanvas.ActualWidth;
            editor.SelectedArea.Height = editor.MainCanvas.ActualHeight;
        }
    }
}
