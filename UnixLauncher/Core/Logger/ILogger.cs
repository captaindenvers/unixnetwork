using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Logger
{
    public interface ILogger : IDisposable
    {
        bool IsEnabled(LogLevel level);
        IDisposable BeginScope<TState>(TState state);
        Task TraceAsync(string message);
        Task DebugAsync(string message);
        Task InfoAsync(string message);
        Task WarnAsync(string message);
        Task ErrorAsync(string message);
        Task ErrorAsync(string message,  Exception exception);
        Task ErrorAsync(Exception exception);
    }
}
