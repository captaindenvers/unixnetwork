namespace UnixLauncher.Core.Tokens
{
    interface ITokenServise
    {
        Task<(Token newRefresh, Token newAuth)> RefreshTokensAsync(Token currentRefresh);

        // TODO:
        // - Заменить object на тип ответа
        // мб вообще создать свой класс под это дело
        // либо же JSON (вероятнее всего)
        Task<object> RequestTokenInfoAsync(Token token);

        // TODO:
        // - согласовать какие данные нужно передавать
        Task<Token> RequestRefresh(string login, string password);

        Task<Token> RequestAuth(Token refreshToken);
    }
}
