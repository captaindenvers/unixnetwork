using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UnixLauncher.Controls
{
    /// <summary>
    /// Логика взаимодействия для WindowControlPanel.xaml
    /// </summary>
    public partial class WindowControlPanel : UserControl
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out System.Drawing.Point lpPoint);


        private System.Drawing.Point startPoint;
        private Point startWindowPosition;
        private bool isDragging = false;

        public WindowControlPanel()
        {
            InitializeComponent();

            // Прикрепляем обработчики событий к элементу DockPanel (определён в XAML с x:Name="DockPanel")
            this.DockPanel.MouseDown += DockPanel_MouseDown;
            this.DockPanel.MouseUp += DockPanel_MouseUp;
            this.DockPanel.MouseMove += DockPanel_MouseMove;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Record the initial mouse position (in screen coordinates) and window position
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Using GetCursorPos to record the starting screen point.
                GetCursorPos(out startPoint);
                Window window = Window.GetWindow(this);
                if (window != null)
                {
                    startWindowPosition = new Point(window.Left, window.Top);
                }
                isDragging = true;
            }
        }

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                // Get current mouse position from the Win32 API (in screen coordinates)
                if (GetCursorPos(out System.Drawing.Point currentPos))
                {
                    double deltaX = currentPos.X - startPoint.X;
                    double deltaY = currentPos.Y - startPoint.Y;

                    Window window = Window.GetWindow(this);
                    if (window != null)
                    {
                        window.Left = startWindowPosition.X + deltaX;
                        window.Top = startWindowPosition.Y + deltaY;
                    }
                }
            }
        }

        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем родительское окно и закрываем его
            Window window = Window.GetWindow(this);
            window?.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем родительское окно и минимизируем его
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
