using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Logger
{
    record class LoggerOptions
    {
        public LogLevel MinLogLevel;
        public string? Directory;
        public string? FileName;
    }
}
