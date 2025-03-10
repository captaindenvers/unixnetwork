namespace UnixLauncher.Core.Tokens
{
    interface ITokensManager : IAuthTokenManager, IRefreshTokenManager
    {
    }

    interface IAuthTokenManager
    {
        public Token GetAuth();
        public bool SetAuth(Token token);
    }
    interface IRefreshTokenManager
    {
        public Token GetRefresh();
        public bool SetRefresh(Token token);
    }
}