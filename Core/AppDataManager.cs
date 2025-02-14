using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace UnixLauncher.Core
{
    static class AppDataManager
    {
        private static readonly string _companyName = "UnixNetwork";

        /// <summary>
        /// Вычисляет путь до папки "UnixNetwork" в AppData
        /// </summary>
        /// <returns>Возвращает строку абсолютного пути</returns>
        public static string GetFolder() 
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{_companyName}\\";
        }
    }
}
