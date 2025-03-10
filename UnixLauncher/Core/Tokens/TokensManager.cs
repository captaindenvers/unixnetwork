using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Tokens
{
    class TokensManager : ITokensManager
    {
        private static readonly Lazy<ITokensManager> _instance =
            new(() => new TokensManager());

        public static ITokensManager Instance { get { return _instance.Value; } }

        public string AuthFileName { get; private set; } = "auth.jwt";
        public string RefreshFilename { get; private set; } = "refresh.jwt";

        public string PathToFiles { get; private set; } = AppDataProvider.GetFolder() + "userSecret\\";

        private TokensManager() { }

        public TokensManager(string authFileName, string refreshFilename, string pathToFiles)
        {
            AuthFileName = authFileName;
            RefreshFilename = refreshFilename;
            PathToFiles = pathToFiles;
        }

        public Token GetAuth()
        {
            throw new NotImplementedException();
        }

        public Token GetRefresh()
        {
            throw new NotImplementedException();
        }

        public bool SetAuth(Token token)
        {
            throw new NotImplementedException();
        }

        public bool SetRefresh(Token token)
        {
            throw new NotImplementedException();
        }
    }
}
