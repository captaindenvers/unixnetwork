namespace UnixLauncher.Core.Tokens
{
    interface ITokenServise
    {
        // TODO:
        // - Заменить string на тип ответа
        // мб вообще создать свой класс под это дело
        // либо же JSON (вероятнее всего)
        Task<string> RequestTokenInfoAsync(string token);

        Task<string> RequestAuth(string login, string password);
    }
}
