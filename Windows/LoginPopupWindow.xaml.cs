using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UnixLauncher.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginPopupWindow.xaml
    /// </summary>
    public partial class LoginPopupWindow : Window
    {
        public LoginPopupWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"--- DEBUG PURPOSES ONLY\n\nLogin: {loginTextBox.Text}\nPassword: {passwordTextBox.Text}");
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
