using System.Windows;
using UnixLauncher.Windows;

namespace UnixLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // --- DI
            

            // --- Включаем основное окно
            Window mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
