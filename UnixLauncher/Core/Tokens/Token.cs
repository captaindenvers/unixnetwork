using System.IdentityModel.Tokens.Jwt;
namespace UnixLauncher.Core.Tokens
{
    /// <summary>
    /// Класс, отвечающий за тип данных JWT токена с некоторыми предварительными
    /// проверками и информацией.
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

        /// <summary>
        /// Создает токен из строки.
        /// </summary>
        /// <param name="token">токен в формате string</param>
        /// <returns>(Token) - токен</returns>
        public static Token FromString(string token) { return new(token);  }

        /// <summary>(Overrided)</summary>
        /// <returns>(string) - возвращает строковое воплощение jwt токена</returns>
        public override string ToString() => AccessToken;
    }
}