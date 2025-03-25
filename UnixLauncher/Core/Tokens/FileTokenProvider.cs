using System.IO;
using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Tokens
{
    class FileTokenProvider : ITokensProvider
    {
        private readonly string _tokenDirectory;
        private readonly string _authFileName = "auth.jwt";
        private readonly string _refreshFileName = "refresh.jwt";

        public FileTokenProvider(TokenProviderOptions options)
        {
            _tokenDirectory = options.TokenDirectory;
            EnsureDirectoryExists(_tokenDirectory);
        }

        // --- Реализация интерфейса
        public async Task<Token> GetAuthTokenAsync() =>
            await ReadTokenAsync(Path.Combine(_tokenDirectory, _authFileName));

        public async Task<Token> GetRefreshTokenAsync() =>
            await ReadTokenAsync(Path.Combine(_tokenDirectory, _refreshFileName));

        public async Task SaveAuthTokenAsync(Token token) =>
            await WriteTokenAsync(Path.Combine(_tokenDirectory, _authFileName), token);

        public async Task SaveRefreshTokenAsync(Token token) =>
            await WriteTokenAsync(Path.Combine(_tokenDirectory, _refreshFileName), token);

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
        private async Task<Token> ReadTokenAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return Token.Empty;

            var content = await File.ReadAllTextAsync(filePath);
            return Token.FromString(content);
        }

        /// <summary>
        /// Записывает токен в файл. Автоматически перезаписывает и создает файл
        /// </summary>
        /// <param name="filePath">Полный путь к файлу</param>
        /// <param name="token">Токен</param>
        private async Task WriteTokenAsync(string filePath, Token token)
        {
            await File.WriteAllTextAsync(filePath, token.ToString());
        }
    }
}
