using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core
{
    /// <summary>
    /// Основные настройки подтверждения данных
    /// </summary>
    internal class ValidatorSettings(char[] AllowedSymbols, int MaxLenght, int MinLenght)
    {
        public char[] AllowedSymbols { get; init; } = AllowedSymbols;
        public int MaxLenght { get; init; } = MaxLenght;
        public int MinLenght { get; init; } = MinLenght;
    }

    /// <summary>
    /// Заданные значения настроек для полей ввода данных
    /// </summary>
    internal class PresetsValidatorSettings
    {
        // Массивы допустимых символов для проверки данных
        private static readonly char[] _allowedLoginSymbols =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();
        private static readonly char[] _allowedPasswordSymbols =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_!@#$%^&*()-+=<>?/\\|[]{}".ToCharArray();

        /// <summary>
        /// Готовые настройки логина с максимальной длиной 16 и минимальной 3
        /// </summary>
        public static ValidatorSettings LoginSettings { get; } = new(_allowedLoginSymbols, 16, 3);
        
        /// <summary>
        /// Готовые настройки пароля с максимальной длиной 32 и минимальной 7
        /// </summary>
        public static ValidatorSettings PasswordSettings { get; } = new(_allowedPasswordSymbols, 32, 7);
    }
}
