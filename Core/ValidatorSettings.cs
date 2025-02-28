using UnixLauncher.Core.Exceptions;

namespace UnixLauncher.Core
{
    
    internal class ValidatorSettings
    {
        public char[] AllowedSymbols { get; init; }
        public int MaxLength { get; init; }
        public int MinLength { get; init; }

        /// <exception cref="ValidatorException">Выбрасывается при недопустимых значениях параметров.</exception>
        public ValidatorSettings(char[] allowedSymbols, int maxLength, int minLength)
        {
            // --- Проверки
            if (allowedSymbols == null)
                throw new ValidatorException("Allowed symbols array must not be null.", int.MinValue);

            if (allowedSymbols.Length < 1)
                throw new ValidatorException("There must be at least 1 allowed symbol.", allowedSymbols.Length);

            if (maxLength < 1)
                throw new ValidatorException("Max length lower 1.", maxLength);

            if (minLength < 0)
                throw new ValidatorException("Min length lower 0.", minLength);

            if (minLength > maxLength)
                throw new ValidatorException($"MinLength ({minLength}) bigger maxLength ({maxLength}).", int.MinValue);
            

            // --- Инициализация
            this.AllowedSymbols = allowedSymbols;
            this.MaxLength = maxLength;
            this.MinLength = minLength;
        }
    }

    internal static class PresetsValidatorSettings
    {
        private static readonly char[] _allowedLoginSymbols =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();

        private static readonly char[] _allowedPasswordSymbols =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_!@#$%^&*()-+=<>?/\\|[]{}".ToCharArray();

        public static ValidatorSettings LoginSettings { get; } = new(_allowedLoginSymbols, 16, 3);
        public static ValidatorSettings PasswordSettings { get; } = new(_allowedPasswordSymbols, 32, 7);
    }
}
