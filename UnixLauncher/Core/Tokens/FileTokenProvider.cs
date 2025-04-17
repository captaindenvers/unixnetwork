using System.IO;

namespace UnixLauncher.Core.Tokens
{
    class FileTokenProvider : ITokensProvider
    {
        private readonly string _tokenDirectory;
        private readonly string _authFileName = "auth.token";
        

        public FileTokenProvider(TokenProviderOptions options)
        {
            _tokenDirectory = options.TokenDirectory;
            EnsureDirectoryExists(_tokenDirectory);
        }

        // --- Реализация интерфейса
        public async Task<string> GetAuthTokenAsync() =>
            await ReadTokenAsync(Path.Combine(_tokenDirectory, _authFileName));

        public async Task SaveAuthTokenAsync(string token) =>
            await WriteTokenAsync(Path.Combine(_tokenDirectory, _authFileName), token);


        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        // --- Методы для упрощения
        /// <summary>
        /// Читает токен из файла. Предварительно проверяет его наличие.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу</param>
        /// <returns>Токен</returns>
        private async Task<string> ReadTokenAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return string.Empty;

            var content = await File.ReadAllTextAsync(filePath);
            return content;
        }

        /// <summary>
        /// Записывает токен в файл. Автоматически перезаписывает и создает файл
        /// </summary>
        /// <param name="filePath">Полный путь к файлу</param>
        /// <param name="token">Токен</param>
        private async Task WriteTokenAsync(string filePath, string token)
        {
            await File.WriteAllTextAsync(filePath, token);
        }
    }
}
