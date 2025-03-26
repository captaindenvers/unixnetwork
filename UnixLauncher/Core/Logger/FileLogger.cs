using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Logger
{
    class FileLogger : ILogger
    {
        private LogLevel _minLogLevel;

        private List<string> _scopes;
       
        private StringBuilder _sbLogMessage;

        private string _fileName = "logs.txt";

        private StreamWriter _fileStream;

        private bool _disposed;

        public FileLogger(LoggerOptions options)
        {
            _minLogLevel = options.MinLogLevel;
            _scopes = new List<string>();
            _sbLogMessage = new();

            // Выбор рабочей папки
            string fullPathWithName;
            if (options.Directory != null && options.FileName != null)
                fullPathWithName = Path.Combine(options.Directory, options.FileName);            
            else
                fullPathWithName = Path.Combine(AppDataProvider.GetFolder(), _fileName);
            
            _fileStream = new(fullPathWithName, true);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
                return null!;

            string scope = state.ToString()!;

            if (string.IsNullOrEmpty(scope))
                return null!;

            _scopes.Add(scope);

            return new ScopeDisposable(scope, _scopes);
        }
        public bool IsEnabled(LogLevel level)
        {
            return level >= _minLogLevel;
        }

        public async Task TraceAsync(string message)
            => await Log(LogLevel.Trace, message);


        public async Task DebugAsync(string message)
            => await Log(LogLevel.Debug, message);
        
        public async Task InfoAsync(string message)
            => await Log(LogLevel.Info, message);

        public async Task WarnAsync(string message)
            => await Log(LogLevel.Warn, message);

        public async Task ErrorAsync(string message)
            => await Log(LogLevel.Error, message);

        public async Task ErrorAsync(string message, Exception exception)
            => await Log(LogLevel.Error, message, exception);

        private async Task Log(LogLevel level, string message, Exception? exception = null)
        {
            if (!IsEnabled(level))
                return;

            string logString = GenerateLogString(level, message, exception);

            Console.WriteLine(logString);
            try
            {
                await _fileStream.WriteLineAsync(logString);
            }
            catch (Exception ex)
            {
                EmergencyResponse("[ERROR] {FileLogger} Ошибка во время записи в файл!", ex);
            }
        }      
        
        private string GenerateLogString(LogLevel level, string message, Exception? exception = null)
        {
            string result;

            // Время
            _sbLogMessage.Append($"{DateTime.Now} ");

            // Log level
            _sbLogMessage.Append($"[{level.ToString()}] ");

            // Скоупы
            foreach (var scope in _scopes)
                _sbLogMessage.Append($"<{scope}> ");

            // Сообщение
            _sbLogMessage.Append(message);

            // Ошибка
            if (exception != null)
                _sbLogMessage.Append(exception.ToString());

            result = _sbLogMessage.ToString();
            _sbLogMessage.Clear();

            return result;
        }

        /// <summary>
        /// Если что-то произойдет во время работы логера, то это сохранится 
        /// в файл рядом с программой, чтобы это можно было отследить
        /// </summary>
        private void EmergencyResponse(string message, Exception exception)
        {
            string fileName = "loggerCrashOutput.txt";

            StreamWriter streamWriter = new(fileName, true);

            streamWriter.WriteLine(message);
            streamWriter.WriteLine(exception.ToString());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _fileStream?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
