using System.IO;
using System.Text;
using UnixLauncher.Core.Misc;
using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Logger
{
    class FileLogger : ILogger, IAsyncDisposable
    {
        private readonly object _sbLock = new();
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly object _scopesLock = new();

        private volatile LogLevel _minLogLevel;
        private readonly string _logFilePath;
        private volatile bool _disposed;

        private readonly List<string> _scopes;
        private readonly StringBuilder _sbLogMessage;

        public FileLogger(LoggerOptions options)
        {
            _minLogLevel = options.MinLogLevel;
            _scopes = new List<string>();
            _sbLogMessage = new StringBuilder();
            _disposed = false;

            // Выбор рабочей папки
            if (options.Directory != null && options.FileName != null)
                _logFilePath = Path.Combine(options.Directory, options.FileName);
            else
                _logFilePath = Path.Combine(AppDataProvider.GetFolder(), "logs.txt");

            // Удостоверяемся, что целевая директория доступна и по необходимости создаем
            string directory = Path.GetDirectoryName(_logFilePath)!;
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
                return NullDisposable.Instance;

            string? scope = state.ToString();

            if (string.IsNullOrEmpty(scope))
                return NullDisposable.Instance;

            lock (_scopesLock)
            {
                _scopes.Add(scope);
            }

            return new ScopeDisposable(scope, _scopes, _scopesLock);
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(FileLogger));

            if (!IsEnabled(level))
                return;

            string logString = GenerateLogString(level, message, exception);

            try
            {
                await _writeLock.WaitAsync();
                try
                {
                    if (_disposed) // Повторная проверка внутри блокировки
                        return;

                    // FileShare.ReadWrite позволяет читать файл другим процессам во время записи
                    using (var fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        await writer.WriteLineAsync(logString);
                        await writer.FlushAsync();
                    }
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            catch (Exception ex)
            {
                EmergencyResponse("[ERROR] {FileLogger} Ошибка во время записи в файл!", ex);
            }
        }

        /// <summary>
        /// Собирает строку для записи в логи
        /// </summary>
        /// <param name="level">Уровень логирования</param>
        /// <param name="message">Непосредственное сообщение</param>
        /// <param name="exception">Ошибка. Может быть null. Стандартное значение null.</param>
        private string GenerateLogString(LogLevel level, string message, Exception? exception = null)
        {
            string result;

            lock (_sbLock)
            {
                // Время
                _sbLogMessage.Append($"{DateTime.Now} ");

                // Log level
                _sbLogMessage.Append($"[{level.ToString()}] ");

                // Скоупы
                lock (_scopesLock)
                {
                    foreach (var scope in _scopes)
                        _sbLogMessage.Append($"<{scope}> ");
                }

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

            try
            {
                // Используем блокировку на файл для потокобезопасности
                using (var fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine(message);
                    streamWriter.WriteLine(exception.ToString());
                }
            }
            catch (Exception e2)
            {
                // В случае критической ошибки ничего не делаем - метод используется как последнее средство
                // ну, а че вы хотели? рекурсивный вызов самого себя до того момента, пока не переполнится стек?
                // Можем только помолиться, чтобы этого не происходило.
                //
                //          МОЛИТВА ПРОГРАММИСТИЧЕСКАЯ #1
                // О, всемогущий FileStream и святой StreamWriter,
                // Храни нас от хаоса многопоточности,
                // Пусть loggerCrashOutput.txt примет все наши грехи ошибок,
                // А Exception.ToString() изольет свет истины.
                // Если ж Catch зовёт нас в бездну рекурсии,
                // И стек уже готов добавить ещё один фрейм,
                // Пошли нам благословение – StackOverflow обойди стороной,
                // А если нет – лишь тихо прошепчи:
                EmergencyResponse("пизда рулю...", e2);
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
                    _writeLock.Dispose();
                }

                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _writeLock.Dispose();
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}