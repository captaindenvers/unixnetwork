using System.IO;

namespace UnixLauncher.Core.Providers
{
    static class AppDataProvider
    {
        private static readonly string _companyName = "UnixNetwork";

        // "Ленивое" вычисление пути (1 раз и тогда, когда потребуется)
        private static readonly Lazy<string> _path = new(() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _companyName));

        /// <summary>
        /// Вычисляет путь до папки приложения в AppData
        /// </summary>
        /// <returns>Возвращает строку абсолютного пути</returns>
        public static string GetFolder()
        {
            return _path.Value;
        }
    }
}
