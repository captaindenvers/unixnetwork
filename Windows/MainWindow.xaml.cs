using System.Windows;
using System.Windows.Input;

namespace UnixLauncher.Windows
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false; // Флаг для отслеживания состояния перетаскивания
        private Point startPoint;        // Начальная позиция курсора (экранные координаты)
        private Point startWindowPosition; // Начальное положение окна

        public MainWindow()
        {
            InitializeComponent();
            this.DockPanel.MouseDown += DockPanel_MouseDown;
            this.DockPanel.MouseUp += DockPanel_MouseUp;
            this.DockPanel.MouseMove += DockPanel_MouseMove;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Получаем начальные экранные координаты курсора
                startPoint = this.PointToScreen(e.GetPosition(this));
                // Запоминаем текущее положение окна
                startWindowPosition = new Point(this.Left, this.Top);
                isDragging = true;

                // Захватываем мышь, чтобы получать события даже при выходе курсора за пределы окна
                DockPanel.CaptureMouse();
            }
        }

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                // Текущие экранные координаты курсора
                Point currentScreenPoint = this.PointToScreen(e.GetPosition(this));

                // Вычисляем разницу между текущей позицией и начальной
                double deltaX = currentScreenPoint.X - startPoint.X;
                double deltaY = currentScreenPoint.Y - startPoint.Y;

                // Применяем смещение к окну
                this.Left = startWindowPosition.X + deltaX;
                this.Top = startWindowPosition.Y + deltaY;
            }
        }

        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Сбрасываем флаг перетаскивания и освобождаем захват мыши
            isDragging = false;
            DockPanel.ReleaseMouseCapture();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
