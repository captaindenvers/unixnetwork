namespace UnixLauncher.Core.Tokens
{
    /// <summary>
    /// Функции для хранения токена с локальным хранилищем (жесткий диск)
    /// </summary>
    interface ITokensProvider
    {
        Task<string> GetAuthTokenAsync();
        Task SaveAuthTokenAsync(string token);
    }
}