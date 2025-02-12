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
using UnixLauncher.Core;

namespace UnixLauncher.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginPopupWindow.xaml
    /// </summary>
    public partial class LoginPopupWindow : Window
    {
        DataValidator validator;
        public LoginPopupWindow()
        {
            InitializeComponent();

            validator = new DataValidator();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var (login, password) = (loginTextBox.Text, passwordTextBox.Text);

            bool isLoginPassCheck = validator.Validate(login, PresetsValidatorSettings.LoginSettings);
            bool isPasswordPassCheck = validator.Validate(password, PresetsValidatorSettings.PasswordSettings);

            if (isLoginPassCheck || isPasswordPassCheck)
            {
                MessageBox.Show("Проверь логин/пароль, еблан. В логине не должно быть спец. символов плюс меньше 16 символов, больше 3.\n\nПароль меньше 32, больше 8.");
                return;
            }

            MessageBox.Show($"--- DEBUG PURPOSES ONLY\n\nLogin: {loginTextBox.Text}\nPassword: {passwordTextBox.Text}");
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
