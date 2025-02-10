using System;
using System.Windows;

namespace UnixLauncher.Windows
{
    public partial class MainWindow : Window
    {
        private LoginPopupWindow _loginPopupWindow;

        public MainWindow()
        {
            InitializeComponent();
            LocationChanged += MainWindow_LocationChanged;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_loginPopupWindow != null)
                return;

            _loginPopupWindow = new LoginPopupWindow
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false
            };

            UpdateLoginPopupPosition();

            // При закрытии окна сбрасываем ссылку, чтобы можно было открыть его снова
            _loginPopupWindow.Closed += (s, args) => _loginPopupWindow = null;
            _loginPopupWindow.Show();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            UpdateLoginPopupPosition();
        }

        private void UpdateLoginPopupPosition()
        {
            if (_loginPopupWindow == null)
                return;

            _loginPopupWindow.Left = Left + (Width - _loginPopupWindow.Width) / 2;
            _loginPopupWindow.Top = Top + (Height - _loginPopupWindow.Height) / 2;
        }
    }
}
