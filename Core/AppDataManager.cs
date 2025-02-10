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

        public static string GetFolder() 
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{_companyName}\\";
        }
    }
}
