using System.IO;
using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Tokens
{
    class FileTokenProvider : ITokensProvider
    {
        private readonly string _tokenDirectory; // = Path.Combine(AppDataProvider.GetFolder(), "User Secret")
        private readonly string _authFileName = "auth.jwt";
        private readonly string _refreshFileName = "refresh.jwt";

        public FileTokenProvider(TokenProviderOptions options)
        {
            _tokenDirectory = options.TokenDirectory;
            EnsureDirectoryExists(_tokenDirectory);
        }

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

        private async Task<Token> ReadTokenAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return Token.Empty;

            var content = await File.ReadAllTextAsync(filePath);
            // Допустим, Token можно создать из строки
            return Token.FromString(content);
        }

        private async Task WriteTokenAsync(string filePath, Token token)
        {
            await File.WriteAllTextAsync(filePath, token.ToString());
        }
    }
}
