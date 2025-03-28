using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Logger
{
    /// <summary>
    /// Основные данные для использования в реализациях <see cref="ILogger"/>
    /// </summary>
    record class LoggerOptions
    {
        public LogLevel MinLogLevel;
        public string? Directory;
        public string? FileName;
    }
}
