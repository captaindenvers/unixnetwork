using System.IdentityModel.Tokens.Jwt;
namespace UnixLauncher.Core.Tokens
{
    /// <summary>
    /// Используйте ToString(), чтобы получить jwt.
    /// </summary>
    public class Token
    {
        public static readonly Token Empty = Token.FromString(string.Empty);
        public string AccessToken { get; }
        public DateTime? ExpiresAt { get; }

        public Token(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Token value cannot be empty", nameof(accessToken));

            AccessToken = accessToken;

            // Пытаемся декодировать токен и извлечь дату истечения (claim "exp")
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(accessToken))
            {
                var jwtToken = handler.ReadJwtToken(accessToken);
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (expClaim != null && long.TryParse(expClaim, out long exp))
                {
                    // Обычно exp задаётся в секундах с начала эпохи
                    ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                }
            }
        }

        public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow >= ExpiresAt;

        public static Token FromString(string token) { return new(token);  }

        public override string ToString() => AccessToken;
    }
}