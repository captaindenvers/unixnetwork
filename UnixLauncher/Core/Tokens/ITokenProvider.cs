namespace UnixLauncher.Core.Tokens
{
    /// <summary>
    /// Функции для хранения токена с локальным хранилищем (жесткий диск)
    /// </summary>
    interface ITokensProvider
    {
        Task<Token> GetAuthTokenAsync();
        Task<Token> GetRefreshTokenAsync();
        Task SaveAuthTokenAsync(Token token);
        Task SaveRefreshTokenAsync(Token token);
    }
}