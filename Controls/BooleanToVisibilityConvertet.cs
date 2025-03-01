using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UnixLauncher.Controls
{
    /// <summary>
    /// Converts a boolean to a Visibility value.
    /// If Invert is true, then true becomes Collapsed and false becomes Visible.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value is bool b && b;
            if (Invert)
                flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility vis)
            {
                bool result = vis == Visibility.Visible;
                return Invert ? !result : result;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
