using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;

namespace ScreenCraft
{
    public partial class App : Application
    {
        private static GlobalKeyboardHook _globalKeyboardHook;
        private NotifyIcon _notifyIcon = new();
        private static EditorWindow editor;

        public static BitmapImage? screenshotBitmapImage = null;

        public static void KeyHookSwitch(bool isOn)
        {
            if(isOn)
            {
                _globalKeyboardHook.KeyboardPressed += OnKeyboardPressed;
            }
            else
            {
                _globalKeyboardHook.KeyboardPressed -= OnKeyboardPressed;
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _globalKeyboardHook = new GlobalKeyboardHook(new Keys[] { Keys.PrintScreen });

            KeyHookSwitch(true);

            InitializeNotifyIcon();
            InitializeEditorWindow();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        private void InitializeEditorWindow()
        {
            editor = new EditorWindow()
            {
                WindowStyle = WindowStyle.None,
                WindowState = WindowState.Minimized,
            };
            editor.Visibility = Visibility.Hidden;
        }
        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("Res/icon.ico"),
                Visible = true
            };
            _notifyIcon.DoubleClick += (sender, args) => Trace.WriteLine("DoubleClick");

            ContextMenuStrip contextMenu = new();

            // Menu Item 1

            ToolStripMenuItem menuItem1 = new("Option1");
            menuItem1.Click += (sender, eventArgs) =>
            {
                Trace.WriteLine("Option1");
                EditorWindow editorWindow = new();
                editorWindow.Show();
            };
            contextMenu.Items.Add(menuItem1);

            // Menu Item 2

            ToolStripMenuItem menuItem2 = new("Option2");
            menuItem2.Click += (sender, eventArgs) =>
            {
                Trace.WriteLine("Option2");
            };
            contextMenu.Items.Add(menuItem2);

            // Menu Item Exit

            ToolStripMenuItem menuItemExit = new("Exit");
            menuItemExit.Click += (sender, eventArgs) =>
            {
                Trace.WriteLine("Exit");
                Shutdown();
            };
            contextMenu.Items.Add(menuItemExit);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private static void OnKeyboardPressed(object? sender, GlobalKeyboardHookEventArgs e)
        {
            if (e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                Keys loggedKey = e.KeyboardData.Key;
                int loggedVkCode = e.KeyboardData.VirtualCode;
                Trace.WriteLine($"{loggedKey}");

                ShowEditorWindow();
            }
        }
        private static void ShowEditorWindow()
        {
            screenshotBitmapImage = CaptureFullScreen();

            editor.Visibility = Visibility.Visible;
            editor.WindowState = WindowState.Maximized;

            KeyHookSwitch(false);
        }
        public static BitmapImage? CaptureFullScreen()
        {
            if (Screen.PrimaryScreen == null) return null;

            System.Drawing.Rectangle bounds = Screen.PrimaryScreen.Bounds;
            using System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(bounds.Width, bounds.Height);
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);
            }
            BitmapImage bitmapImage = new();
            using (MemoryStream memory = new())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
    }
}
