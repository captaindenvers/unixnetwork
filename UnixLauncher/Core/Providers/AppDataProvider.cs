using System.IO;

namespace UnixLauncher.Core.Managers
{
    static class AppDataProvider
    {
        private static readonly string _companyName = "UnixNetwork";

        /// <summary>
        /// Вычисляет путь до папки "UnixNetwork" в AppData
        /// </summary>
        /// <returns>Возвращает строку абсолютного пути</returns>
        public static string GetFolder() 
        {
            return Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                _companyName);
        }
    }
}
