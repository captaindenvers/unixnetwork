using System.Windows;
using System.Windows.Controls;

namespace UnixLauncher.Controls
{
    public partial class LoginEntryControl : UserControl
    {
        public LoginEntryControl()
        {
            InitializeComponent();
            Loaded += LoginEntryControl_Loaded;
        }

        private void LoginEntryControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the initial text for the control when it loads.
            if (!string.IsNullOrEmpty(InitialText))
            {
                if (IsPassword)
                    pwdInput.Password = InitialText;
                else
                    txtInput.Text = InitialText;
            }
        }

        // ExplanatoryText Dependency Property
        public static readonly DependencyProperty ExplanatoryTextProperty =
            DependencyProperty.Register("ExplanatoryText", typeof(string), typeof(LoginEntryControl),
                new PropertyMetadata("Enter value:"));

        public string ExplanatoryText
        {
            get => (string)GetValue(ExplanatoryTextProperty);
            set => SetValue(ExplanatoryTextProperty, value);
        }

        // InitialText Dependency Property
        public static readonly DependencyProperty InitialTextProperty =
            DependencyProperty.Register("InitialText", typeof(string), typeof(LoginEntryControl),
                new PropertyMetadata(string.Empty));

        public string InitialText
        {
            get => (string)GetValue(InitialTextProperty);
            set => SetValue(InitialTextProperty, value);
        }

        // IsPassword Dependency Property
        public static readonly DependencyProperty IsPasswordProperty =
            DependencyProperty.Register("IsPassword", typeof(bool), typeof(LoginEntryControl),
                new PropertyMetadata(false));

        public bool IsPassword
        {
            get => (bool)GetValue(IsPasswordProperty);
            set => SetValue(IsPasswordProperty, value);
        }

        // Text Dependency Property
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LoginEntryControl),
                new PropertyMetadata(string.Empty));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Update the Text property when the PasswordBox content changes.
        private void pwdInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Text = pwdInput.Password;
        }
    }
}
