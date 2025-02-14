using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UnixLauncher.Controls
{
    /// <summary>
    /// Логика взаимодействия для WindowControlPanel.xaml
    /// </summary>
    public partial class WindowControlPanel : UserControl
    {
        private bool isDragging = false;          // Флаг для отслеживания состояния перетаскивания
        private Point startPoint;                 // Начальная позиция курсора (в экранных координатах)
        private Point startWindowPosition;        // Начальное положение окна

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
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Получаем родительское окно и запоминаем его позицию
                Window window = Window.GetWindow(this);
                if (window != null)
                {
                    startWindowPosition = new Point(window.Left, window.Top);
                }

                isDragging = true;

                // Захватываем мышь, чтобы получать события даже при выходе курсора за пределы элемента
                this.DockPanel.CaptureMouse();
            }
        }

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                // Текущие экранные координаты курсора
                Point relativePos = e.GetPosition(this);
                var dpi = VisualTreeHelper.GetDpi(this);
                Point screenPos = new Point(relativePos.X * dpi.DpiScaleX, relativePos.Y * dpi.DpiScaleY);

                // Вычисляем смещение относительно начальной точки
                double deltaX = screenPos.X - startPoint.X;
                double deltaY = screenPos.Y - startPoint.Y;

                // Получаем родительское окно и изменяем его позицию
                Window window = Window.GetWindow(this);
                if (window != null)
                {
                    window.Left = startWindowPosition.X + deltaX;
                    window.Top = startWindowPosition.Y + deltaY;
                }
            }
        }

        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Сбрасываем флаг перетаскивания и освобождаем захват мыши
            isDragging = false;
            this.DockPanel.ReleaseMouseCapture();
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
