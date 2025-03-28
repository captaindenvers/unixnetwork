using UnixLauncher.Core.Logger;

namespace UnixLauncher.Tests
{
    public class FileLoggerTests
    {
        private const string TestDirectory = "TestLogs";

        private string GenerateFileName() { return $"test_logs_{Guid.NewGuid()}.txt"; }

        private string GetLogFilePath(string fileName) => Path.Combine(TestDirectory, fileName);

        private FileLogger CreateLogger(string fileName, LogLevel minLogLevel = LogLevel.Trace)
        {
            var options = new LoggerOptions
            {
                MinLogLevel = minLogLevel,
                Directory = TestDirectory,
                FileName = fileName
            };
            return new FileLogger(options);
        }

        [Fact]
        public async Task LogsMessageAtCorrectLevel()
        {
            string fileName = GenerateFileName();
            using (var logger = CreateLogger(fileName, LogLevel.Info))
            {
                await logger.InfoAsync("Info message");
                await logger.DebugAsync("Debug message");
            }

            // Убеждаемся, что логгер корректно завершил работу
            // Читаем файл только после полного освобождения ресурса
            string logContent = await File.ReadAllTextAsync(GetLogFilePath(fileName));
            Assert.Contains("Info message", logContent);
            Assert.DoesNotContain("Debug message", logContent);
        }


        [Fact]
        public async Task LogsWithScope()
        {
            string fileName = GenerateFileName();
            var logger = CreateLogger(fileName);
            using (logger.BeginScope("TestScope"))
            {
                await logger.InfoAsync("Scoped message");
            }
            logger.Dispose();

            string logContent = await File.ReadAllTextAsync(GetLogFilePath(fileName));
            Assert.Contains("<TestScope>", logContent);
        }

        [Fact]
        public async Task HandlesLoggingExceptionGracefully()
        {
            string fileName = GenerateFileName();
            var logger = CreateLogger(fileName);
            await logger.ErrorAsync("Error message", new Exception("Test Exception"));
            logger.Dispose();

            string logContent = await File.ReadAllTextAsync(GetLogFilePath(fileName));
            Assert.Contains("Error message", logContent);
            Assert.Contains("Test Exception", logContent);
        }

        [Fact]
        public void Dispose_ClosesStreamWithoutErrors()
        {
            string fileName = GenerateFileName();
            var logger = CreateLogger(fileName);
            logger.Dispose();

            // Should not throw exception on second Dispose
            logger.Dispose();
        }

        [Fact]
        public void IsEnabled_ReturnsCorrectValues()
        {
            string fileName = GenerateFileName();
            var logger = CreateLogger(fileName, LogLevel.Warn);
            Assert.False(logger.IsEnabled(LogLevel.Info));
            Assert.True(logger.IsEnabled(LogLevel.Warn));
        }
    }
}
