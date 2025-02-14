using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core
{
    internal class ValidatorSettings(char[] AllowedSymbols, int MaxLenght, int MinLenght)
    {
        public char[] AllowedSymbols { get; init; } = AllowedSymbols;
        public int MaxLenght { get; init; } = MaxLenght;
        public int MinLenght { get; init; } = MinLenght;
    }

    internal class PresetsValidatorSettings
    {
        private static readonly char[] _allowedLoginSymbols =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_".ToCharArray();

        private static readonly char[] _allowedPasswordSymbols =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_!@#$%^&*()-+=<>?/\\|[]{}".ToCharArray();

        public static ValidatorSettings LoginSettings { get; } = new(_allowedLoginSymbols, 16, 3);
        public static ValidatorSettings PasswordSettings { get; } = new(_allowedPasswordSymbols, 32, 7);
    }
}
