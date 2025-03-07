using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnixLauncher.Core.Exceptions;
using UnixLauncher.Core.Validator;

namespace UnixLauncher.Tests
{
    public class DataValidatorTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var allowedSymbols = new[] { 'a', 'b', 'c' };
            const int maxLength = 10;
            const int minLength = 3;

            // Act
            var settings = new ValidatorSettings(allowedSymbols, maxLength, minLength);

            // Assert
            Assert.Equal(allowedSymbols, settings.AllowedSymbols);
            Assert.Equal(maxLength, settings.MaxLength);
            Assert.Equal(minLength, settings.MinLength);
        }

        [Fact]
        public void Constructor_NullAllowedSymbols_ThrowsValidatorException()
        {
            // Arrange
            char[] allowedSymbols = null;
            const int maxLength = 10;
            const int minLength = 3;

            // Act & Assert
            var exception = Assert.Throws<ValidatorException>(() =>
                new ValidatorSettings(allowedSymbols, maxLength, minLength));

            Assert.Equal("Allowed symbols array must not be null.", exception.Message);
            Assert.Equal(int.MinValue, exception.Value);
        }

        [Fact]
        public void Constructor_EmptyAllowedSymbols_ThrowsValidatorException()
        {
            // Arrange
            var allowedSymbols = Array.Empty<char>();
            const int maxLength = 10;
            const int minLength = 3;

            // Act & Assert
            var exception = Assert.Throws<ValidatorException>(() =>
                new ValidatorSettings(allowedSymbols, maxLength, minLength));

            Assert.Equal("There must be at least 1 allowed symbol.", exception.Message);
            Assert.Equal(0, exception.Value);
        }

        [Fact]
        public void Constructor_MaxLengthLessThanOne_ThrowsValidatorException()
        {
            // Arrange
            var allowedSymbols = new[] { 'a', 'b', 'c' };
            const int maxLength = 0;
            const int minLength = 0;

            // Act & Assert
            var exception = Assert.Throws<ValidatorException>(() =>
                new ValidatorSettings(allowedSymbols, maxLength, minLength));

            Assert.Equal("Max length lower 1.", exception.Message);
            Assert.Equal(0, exception.Value);
        }

        [Fact]
        public void Constructor_MinLengthLessThanZero_ThrowsValidatorException()
        {
            // Arrange
            var allowedSymbols = new[] { 'a', 'b', 'c' };
            const int maxLength = 10;
            const int minLength = -1;

            // Act & Assert
            var exception = Assert.Throws<ValidatorException>(() =>
                new ValidatorSettings(allowedSymbols, maxLength, minLength));

            Assert.Equal("Min length lower 0.", exception.Message);
            Assert.Equal(-1, exception.Value);
        }

        [Fact]
        public void Constructor_MinLengthGreaterThanMaxLength_ThrowsValidatorException()
        {
            // Arrange
            var allowedSymbols = new[] { 'a', 'b', 'c' };
            const int maxLength = 5;
            const int minLength = 10;

            // Act & Assert
            var exception = Assert.Throws<ValidatorException>(() =>
                new ValidatorSettings(allowedSymbols, maxLength, minLength));

            Assert.Equal($"MinLength ({minLength}) bigger maxLength ({maxLength}).", exception.Message);
            Assert.Equal(int.MinValue, exception.Value);
        }

        [Fact]
        public void PresetsValidatorSettings_LoginSettings_HasCorrectValues()
        {
            // Act
            var loginSettings = PresetsValidatorSettings.LoginSettings;

            // Assert
            Assert.Equal(16, loginSettings.MaxLength);
            Assert.Equal(3, loginSettings.MinLength);
            Assert.Contains('a', loginSettings.AllowedSymbols);
            Assert.Contains('Z', loginSettings.AllowedSymbols);
            Assert.Contains('_', loginSettings.AllowedSymbols);
            Assert.Contains('9', loginSettings.AllowedSymbols);
            Assert.DoesNotContain('@', loginSettings.AllowedSymbols);
        }

        [Fact]
        public void PresetsValidatorSettings_PasswordSettings_HasCorrectValues()
        {
            // Act
            var passwordSettings = PresetsValidatorSettings.PasswordSettings;

            // Assert
            Assert.Equal(32, passwordSettings.MaxLength);
            Assert.Equal(8, passwordSettings.MinLength);
            Assert.Contains('a', passwordSettings.AllowedSymbols);
            Assert.Contains('Z', passwordSettings.AllowedSymbols);
            Assert.Contains('_', passwordSettings.AllowedSymbols);
            Assert.Contains('!', passwordSettings.AllowedSymbols);
            Assert.Contains('@', passwordSettings.AllowedSymbols);
            Assert.DoesNotContain(' ', passwordSettings.AllowedSymbols);
        }
    }
}
