namespace UnixLauncher.Core.Tokens
{
    interface ITokensProvider
    {
        Task<Token> GetAuthTokenAsync();
        Task<Token> GetRefreshTokenAsync();
        Task SaveAuthTokenAsync(Token token);
        Task SaveRefreshTokenAsync(Token token);
    }
}