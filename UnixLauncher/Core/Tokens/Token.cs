namespace UnixLauncher.Core.Tokens
{
    /// <summary>
    /// Используйте ToString(), чтобы получить jwt.
    /// </summary>
    class Token
    {
        public DateTime StartLifeTime { get; private set; }
        public DateTime EndOfLifeTime { get; private set; }
        public TimeSpan LifeTime => EndOfLifeTime - StartLifeTime;
        public TimeSpan RemainingLifeTime => DateTime.Now - StartLifeTime;
        public TokenType TokenType { get; private set; }

        private readonly string _token = string.Empty;

        public Token(string token, TokenType tokenType, DateTime endOfLifeTime)
        {
            _token = token;
            TokenType = tokenType;
            StartLifeTime = DateTime.Now;
            EndOfLifeTime = endOfLifeTime;
        }

        public Token(string token, TokenType tokenType, DateTime startLifeTime, DateTime endOfLifeTime)
        {
            _token = token;
            TokenType = tokenType;
            StartLifeTime = startLifeTime;
            EndOfLifeTime = endOfLifeTime;
        }


        /// <returns> (string) Token</returns>
        public override string ToString()
        {
            return _token;
        }
    }
}
