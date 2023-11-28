using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ScreenCraft
{
    /// <summary>
    /// Логика взаимодействия для EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {

        public Canvas MainCanvas;
        public Image MainImage { get; set; }
        public Image SecondImage { get; set; }
        public Rectangle SelectedArea { get; set; }

        private StackPanel MainMenu;
        private StackPanel ColorPickerPanel;

        List<CheckBox> actionButtons;

        public event EventHandler<ColorChangedEventArgs> ColorPicked;
        public static Color SelectedColor {  get; private set; }
        public static float GlobalSize { get; private set; }
        private static List<UIElement> objects = new();
        public static void ObjectsListAdd(UIElement uIElement)
        {
            objects.Add(uIElement);
        }

        public bool rectDrawingCheck = false;
        public bool isMouseOverRect = false;

        private bool isDrawing = false;

        private readonly EditorContext editorContext = new();

        private ScreenshotAreaPickingState screenshotAreaPickingState;
        private ScreenshotAreaMovingState screenshotAreaMovingState;

        private FreePenDrawingState freePenDrawingState;
        private LineDrawingState lineState;
        private RectangleDrawingState rectangleState;
        private ArrowDrawingState arrowDrawingState;
        private TextDrawingState textDrawingState;


        public EditorWindow()
        {
            InitializeComponent();

            InitializeEditor();

            Loaded += MainWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;

        }

        private void InitializeEditor()
        {
            MainCanvas = new Canvas();
            MainCanvas.Background = Brushes.Black;
            Content = MainCanvas;

            MainImage = new Image();
            Canvas.SetLeft(MainImage, 0);
            Canvas.SetTop(MainImage, 0);
            MainCanvas.Children.Add(MainImage);

            SecondImage = new Image();

            Canvas.SetLeft(SecondImage, 0);
            Canvas.SetTop(SecondImage, 0);
            SecondImage.Opacity = 0.5;
            MainCanvas.Children.Add(SecondImage);


        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            InitializeCanvas();


            SelectedArea = new();
            screenshotAreaPickingState = new(this);
            screenshotAreaMovingState = new(this);

            freePenDrawingState = new FreePenDrawingState(MainCanvas);
            lineState = new LineDrawingState(MainCanvas);
            rectangleState = new RectangleDrawingState(MainCanvas, this);
            arrowDrawingState = new ArrowDrawingState(MainCanvas);
            textDrawingState = new TextDrawingState(MainCanvas);

            editorContext.SetState(screenshotAreaPickingState);

            MouseMove += MainWindow_MouseMove;

            CreateRectMenu();
            Trace.WriteLine("editor Loaded");
        }
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Увеличение или уменьшение значения globalSize в зависимости от направления вращения колеса мыши
            const float step = 0.1f; // Шаг изменения значения
            if (e.Delta > 0)
            {
                GlobalSize = Math.Min(1.0f, GlobalSize + step); // Увеличение значения, но не больше 1
            }
            else
            {
                GlobalSize = Math.Max(0.0f, GlobalSize - step); // Уменьшение значения, но не меньше 0
            }

            // Здесь можно добавить код для обновления интерфейса или других элементов, использующих globalSize
        }
        private void InitializeCanvas()
        {
           // MainCanvas = new Canvas();
           // MainCanvas.Background = Brushes.Black;
          //  Content = MainCanvas;


            BitmapImage? bitmapImage = App.screenshotBitmapImage;

          //  MainImage = new Image();
            MainImage.Source = bitmapImage;

           // Canvas.SetLeft(MainImage, 0);
          //  Canvas.SetTop(MainImage, 0);
          //  MainCanvas.Children.Add(MainImage);

            MainImage.Clip = new RectangleGeometry(new Rect(0, 0, 0, 0));

          //  SecondImage = new Image();
            SecondImage.Source = bitmapImage;

           Canvas.SetLeft(SecondImage, 0);
           Canvas.SetTop(SecondImage, 0);
           SecondImage.Opacity = 0.5;
           MainCanvas.Children.Add(SecondImage);


            MainCanvas.MouseLeftButtonDown += Canvas_MouseDown;
            MainCanvas.MouseMove += Canvas_MouseMove;
            MainCanvas.MouseLeftButtonUp += Canvas_MouseUp;

            Cursor = Cursors.Cross;
        }
        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedArea == null || rectDrawingCheck || isDrawing) return;
            if (SelectedArea.IsMouseOver && !isMouseOverRect)
            {
                Trace.WriteLine("RectArea_MouseEnter");
                RectArea_MouseEnter(SelectedArea, e);
            }
            else if (!SelectedArea.IsMouseOver && isMouseOverRect)
            {
                Trace.WriteLine("RectArea_MouseLeave");
                RectArea_MouseLeave(SelectedArea, e);
            }
        }
        private void RectArea_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOverRect = true;
            Cursor = Cursors.Hand;
            editorContext.SetState(screenshotAreaMovingState);

            /*            SelectedArea.MouseLeftButtonDown += RectArea_MouseLeftButtonDown;
                        SelectedArea.MouseMove += RectArea_MouseMove;
                        SelectedArea.MouseLeftButtonUp += RectArea_MouseLeftButtonUp;

                        mainCanvas.MouseLeftButtonDown -= Canvas_MouseLeftButtonDown;
                        mainCanvas.MouseMove -= Canvas_MouseMove;
                        mainCanvas.MouseLeftButtonUp -= Canvas_MouseLeftButtonUp;*/

        }
        private void RectArea_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOverRect = false;
            Cursor = Cursors.Cross;
            editorContext.SetState(screenshotAreaPickingState);

            /*            SelectedArea.MouseLeftButtonDown -= RectArea_MouseLeftButtonDown;
                        SelectedArea.MouseMove -= RectArea_MouseMove;
                        SelectedArea.MouseLeftButtonUp -= RectArea_MouseLeftButtonUp;

                        mainCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
                        mainCanvas.MouseMove += Canvas_MouseMove;
                        mainCanvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;*/

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            editorContext.HandleMouseDown(sender, e);
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            editorContext.HandleMouseMove(sender, e);
        }
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            editorContext.HandleMouseUp(sender, e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true; // Отменяем стандартное закрытие окна
            ResetEditorValues();
            Hide(); // Скрываем окно вместо закрытия
        }

        private void ResetEditorValues()
        {
            

        }

        public int ResizerType(MouseEventArgs e, Rectangle rectArea)
        {
            if (rectArea == null) return 0;

            Point clickPoint = e.GetPosition(MainCanvas);

            double left = Canvas.GetLeft(rectArea);
            double top = Canvas.GetTop(rectArea);
            double width = rectArea.Width;
            double height = rectArea.Height;

            List<Point> resizePoints = new List<Point>
            {
                new Point(left, top),           // top left
                new Point(left + width, top),           // top right
                new Point(left + width, top + height), // bot right
                new Point(left, top + height),      // bot left

                new Point(left + width / 2, top),   // top mid
                new Point(left + width, top + height / 2), // right mid
                new Point(left + width / 2, top + height), // bot mid
                new Point(left, top + height / 2), // left mid
            };
            double radius = 10;

            for (int i = 0; i < resizePoints.Count; i++)
            {
                Point point = resizePoints[i];
                double distance = Math.Sqrt(Math.Pow(point.X - clickPoint.X, 2) + Math.Pow(point.Y - clickPoint.Y, 2));
                if (distance <= radius)
                {
                    return i + 1;
                }
            }
            return 0;
        }
        public void ShowRectMenu()
        {
            Trace.WriteLine("ShowRectMenu");

            double rectAreaBottom = Canvas.GetTop(SelectedArea) + SelectedArea.ActualHeight;
            double canvasHeight = MainCanvas.ActualHeight;
            if (canvasHeight - rectAreaBottom > MainMenu.Height)
            {
                Canvas.SetTop(MainMenu, rectAreaBottom + 10);
            }
            else
            {
                Canvas.SetTop(MainMenu, Canvas.GetTop(SelectedArea) + SelectedArea.Height - MainMenu.Height - 10);
            }
            Canvas.SetLeft(MainMenu, Canvas.GetLeft(SelectedArea) + SelectedArea.Width / 2 - MainMenu.Width / 2);


            MainMenu.Visibility = Visibility.Visible;
        }
        public void HideRectMenu()
        {
            Trace.WriteLine("HideRectMenu");
            MainMenu.Visibility = Visibility.Collapsed;
        }
        private void ColorPickerControl_ColorPicked(object sender, ColorChangedEventArgs e)
        {
            SelectedColor = e.Color;
            ColorPickerPanel.Background = new SolidColorBrush(SelectedColor);
        }
        public void CreateRectMenu()
        {
            MainMenu = new StackPanel();
           // MainMenu.Width = 500;
            MainMenu.Height = 50;
            MainMenu.Background = new SolidColorBrush(Color.FromArgb(150, 50, 50, 50));
            MainMenu.Orientation = Orientation.Horizontal;

            actionButtons = new();
            CreateMenuButtons();


            Canvas.SetTop(MainMenu, 0);
            Canvas.SetLeft(MainMenu, 0);
            MainCanvas.Children.Add(MainMenu);
            Panel.SetZIndex(MainMenu, 200);

            MainMenu.Visibility = Visibility.Collapsed;
        }

        CheckBox CreateButton(string name, Style style)
        {
            CheckBox r = new CheckBox
            {
                Content = new Image() { Source = new BitmapImage(new Uri($"pack://application:,,,/Res/Buttons/{name}.png")) },
                Style = style,
                IsChecked = isDrawing
            };
            actionButtons.Add(r);
            return r;
        }
        Button CreateActionButton(string name, Style style)
        {
            return new Button
            {
                Content = new Image() { Source = new BitmapImage(new Uri($"pack://application:,,,/Res/Buttons/{name}.png")) },
                Style = style
            };
        }
        void ChangeToolState(object sender, bool drawingState, IEditorState state, Action? additionalAction)
        {
            if (drawingState)
            {
                foreach (CheckBox b in actionButtons)
                {
                    CheckBox check = b as CheckBox;
                    if (check != sender)
                    {
                        check.IsChecked = false;
                    }
                    else
                    {
                        check.IsChecked = true;
                    }
                }
            }
            isDrawing = drawingState;
            editorContext.SetState(state);

            additionalAction?.Invoke();
        }
        private void CreateMenuButtons()
        {
            Style style = (Style)FindResource("MyCheckBoxStyle");
            Style aStyle = (Style)FindResource("MyButtonStyle");

            CheckBox pen = CreateButton("pen", style);
            CheckBox line = CreateButton("line", style);
            CheckBox arrow = CreateButton("arrow", style);
            CheckBox rect = CreateButton("rect", style);
            CheckBox fill = CreateButton("fill", style);
            CheckBox blur = CreateButton("pen", style);
            CheckBox marker = CreateButton("marker", style);
            CheckBox text = CreateButton("text", style);

            Button undo = CreateActionButton("pen", aStyle);
            Button copy = CreateActionButton("copy", aStyle);
            Button save = CreateActionButton("save", aStyle);
            Button exit = CreateActionButton("exit", aStyle);

            Button color = CreateActionButton("exit", aStyle);
            SelectedColor = Colors.Red;
            color.Background = new SolidColorBrush(SelectedColor);


            pen.Checked += (sender, e) => ChangeToolState(sender, true, freePenDrawingState, null);
            pen.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            line.Checked += (sender, e) => ChangeToolState(sender, true, lineState, null);
            line.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            arrow.Checked += (sender, e) => ChangeToolState(sender, true, arrowDrawingState, null);
            arrow.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            text.Checked += (sender, e) => ChangeToolState(sender, true, textDrawingState, null);
            text.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            rect.Checked += (sender, e) =>
            {
                ChangeToolState(sender, true, rectangleState, () =>
                {
                    rectangleState.NofillCheck();
                });
            };
            rect.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            fill.Checked += (sender, e) =>
            {
                ChangeToolState(sender, true, rectangleState, () =>
                {
                    rectangleState.FillCheck();
                });
            };
            fill.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            blur.Checked += (sender, e) =>
            {
                ChangeToolState(sender, true, rectangleState, () =>
                {
                    rectangleState.BlurCheck();
                });
            };
            blur.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);

            marker.Checked += (sender, e) =>
            {
                ChangeToolState(sender, true, freePenDrawingState, () =>
                {
                    freePenDrawingState.MarkerCheck(true);
                });
            };
            marker.Unchecked += (sender, e) => ChangeToolState(sender, false, screenshotAreaMovingState, null);
            
            copy.Click += Copy_Click;
            save.Click += SaveFile_Click;
            undo.Click += Undo_Click;
            exit.Click += Exit_Click;

            color.Click += Color_Click;

            MainMenu.Children.Add(pen);
            MainMenu.Children.Add(line);
            MainMenu.Children.Add(arrow);
            MainMenu.Children.Add(rect);
            MainMenu.Children.Add(fill);
            MainMenu.Children.Add(blur);
            MainMenu.Children.Add(marker);
            MainMenu.Children.Add(text);

            MainMenu.Children.Add(color);

            MainMenu.Children.Add(undo);
            MainMenu.Children.Add(save);
            MainMenu.Children.Add(copy);
            MainMenu.Children.Add(exit);
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            var colorPicker = new System.Windows.Forms.ColorDialog();
            if (colorPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorPicker.Color;
                SelectedColor = Color.FromArgb(color.A, color.R, color.G, color.B);
                ColorPicked?.Invoke(this, new ColorChangedEventArgs(SelectedColor));
            }
            Button b = sender as Button;
            b.Background = new SolidColorBrush(SelectedColor);

        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Copy_Click(object sender, RoutedEventArgs e)
        {

            CopyCanvasToClipboard(MainCanvas, SelectedArea);
            Close();
        }
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {

            SaveCanvasToImage(MainCanvas, SelectedArea);
            Close();
        }
        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (objects.Count > 0)
            {
                UIElement obj = objects[objects.Count - 1];
                MainCanvas.Children.Remove(obj);
                objects.RemoveAt(objects.Count - 1);
            }
        }

        public void UpdateImage()
        {
            if (MainImage == null || SelectedArea == null) return;

            double rectLeft = Canvas.GetLeft(SelectedArea);
            double rectTop = Canvas.GetTop(SelectedArea);
            double rectRight = rectLeft + SelectedArea.Width;
            double rectBottom = rectTop + SelectedArea.Height;

            double imgLeft = Math.Max(0, rectLeft);
            double imgTop = Math.Max(0, rectTop);
            double imgRight = Math.Min(MainImage.ActualWidth, rectRight);
            double imgBottom = Math.Min(MainImage.ActualHeight, rectBottom);

            double visibleWidth = imgRight - imgLeft;
            double visibleHeight = imgBottom - imgTop;

            MainImage.Clip = new RectangleGeometry(new Rect(imgLeft, imgTop, visibleWidth, visibleHeight));
        }

        public static void SaveCanvasToImage(Canvas canvas, Rectangle area)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = "my_image.png";

            if (saveFileDialog.ShowDialog() == true)
            {
                // Создание RenderTargetBitmap с заданными параметрами
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)area.ActualWidth, (int)area.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);

                // Рендеринг содержимого Canvas на RenderTargetBitmap
                renderBitmap.Render(canvas);
                // Выделение области с помощью CroppedBitmap
                int x = (int)Canvas.GetLeft(area);
                int y = (int)Canvas.GetTop(area);
                int width = (int)area.ActualWidth;
                int height = (int)area.ActualHeight;
                CroppedBitmap croppedBitmap = new CroppedBitmap(renderBitmap, new Int32Rect(x, y, width, height));

                // Создание кодера изображения для формата PNG
                PngBitmapEncoder encoder = new PngBitmapEncoder();

                // Добавление RenderTargetBitmap в кодер изображения
                encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));

                // Запись содержимого кодера изображения в файл
                using (FileStream file = File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(file);
                }
            }
        }
        public static void CopyCanvasToClipboard(Canvas canvas, Rectangle area)
        {
            // Создание RenderTargetBitmap с заданными параметрами
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);

            // Рендеринг содержимого Canvas на RenderTargetBitmap
            renderBitmap.Render(canvas);

            // Выделение области с помощью CroppedBitmap
            int x = (int)Canvas.GetLeft(area);
            int y = (int)Canvas.GetTop(area);
            int width = (int)area.ActualWidth;
            int height = (int)area.ActualHeight;
            CroppedBitmap croppedBitmap = new CroppedBitmap(renderBitmap, new Int32Rect(x, y, width, height));

            // Копирование изображения в буфер обмена
            Clipboard.SetImage(croppedBitmap);
        }
    }
    public class ColorChangedEventArgs : EventArgs
    {
        public Color Color { get; }

        public ColorChangedEventArgs(Color color)
        {
            Color = color;
        }
    }
}
