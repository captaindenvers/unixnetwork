using System.IO;
using System.Text;
using UnixLauncher.Core.Misc;
using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Logger
{
    class FileLogger : ILogger
    {
        private readonly object _lock = new();

        private LogLevel _minLogLevel;

        private List<string> _scopes;
       
        private StringBuilder _sbLogMessage;

        private string _fileName = "logs.txt";

        private StreamWriter _fileStream;

        private bool _disposed;

        public FileLogger(LoggerOptions options)
        {
            _minLogLevel = options.MinLogLevel;
            _scopes = new();
            _sbLogMessage = new();

            // Выбор рабочей папки
            string fullPathWithName;

            if (options.Directory != null && options.FileName != null)
                fullPathWithName = Path.Combine(options.Directory, options.FileName);            
            else
                fullPathWithName = Path.Combine(AppDataProvider.GetFolder(), _fileName);

            // Удостоверяемся, что целевая директория доступна и по необходимости создаем
            string directory = Path.GetDirectoryName(fullPathWithName)!;
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _fileStream = new(fullPathWithName, true);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
                return NullDisposable.Instance;

            string? scope = state.ToString();

            if (string.IsNullOrEmpty(scope))
                return NullDisposable.Instance;

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

        public async Task ErrorAsync(Exception exception)
            => await Log(LogLevel.Error, exception.Message, exception);

        private async Task Log(LogLevel level, string message, Exception? exception = null)
        {
            if (!IsEnabled(level))
                return;

            string logString = GenerateLogString(level, message, exception);
            
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

            lock (_lock)
            {
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
            }

            return result;
        }

        /// <summary>
        /// Если что-то произойдет во время работы логера, то это сохранится 
        /// в файл рядом с программой, чтобы это можно было отследить
        /// </summary>
        private void EmergencyResponse(string message, Exception exception)
        {
            string fileName = "loggerCrashOutput.txt";

            using (StreamWriter streamWriter = new(fileName, true))
            {
                streamWriter.WriteLine(message);
                streamWriter.WriteLine(exception.ToString());
            }
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
