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
        private readonly DataValidator _validator;

        public DataValidatorTests()
        {
            _validator = new DataValidator();
        }

        [Fact]
        public void Validate_NullText_ReturnsFalse()
        {
            // Arrange
            string text = null;
            var settings = new ValidatorSettings(new[] { 'a' }, 10, 1);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_EmptyText_ReturnsFalse()
        {
            // Arrange
            var text = string.Empty;
            var settings = new ValidatorSettings(new[] { 'a' }, 10, 1);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_TextTooLong_ReturnsFalse()
        {
            // Arrange
            var text = "abcdefghijk"; // 11 chars
            var settings = new ValidatorSettings(new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k' }, 10, 1);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_TextTooShort_ReturnsFalse()
        {
            // Arrange
            var text = "ab"; // 2 chars
            var settings = new ValidatorSettings(new[] { 'a', 'b' }, 10, 3);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_ContainsDisallowedSymbols_ReturnsFalse()
        {
            // Arrange
            var text = "abc123";
            var settings = new ValidatorSettings(new[] { 'a', 'b', 'c' }, 10, 1);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Validate_ValidText_ReturnsTrue()
        {
            // Arrange
            var text = "abc123";
            var settings = new ValidatorSettings(new[] { 'a', 'b', 'c', '1', '2', '3' }, 10, 1);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_TextExactlyMinLength_ReturnsTrue()
        {
            // Arrange
            var text = "abc"; // 3 chars
            var settings = new ValidatorSettings(new[] { 'a', 'b', 'c' }, 10, 3);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_TextExactlyMaxLength_ReturnsTrue()
        {
            // Arrange
            var text = "abcdefghij"; // 10 chars
            var settings = new ValidatorSettings(
                new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' },
                10,
                1);

            // Act
            var result = _validator.Validate(text, settings);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_WithLoginPreset_ValidatesCorrectly()
        {
            // Arrange
            var validator = new DataValidator();

            // Act & Assert
            Assert.True(validator.Validate("user_123", PresetsValidatorSettings.LoginSettings));
            Assert.False(validator.Validate("us", PresetsValidatorSettings.LoginSettings)); // Too short
            Assert.False(validator.Validate("user@123", PresetsValidatorSettings.LoginSettings)); // Invalid char
            Assert.False(validator.Validate("abcdefghijklmnopqrstuvwxyz", PresetsValidatorSettings.LoginSettings)); // Too long
        }

        [Fact]
        public void Validate_WithPasswordPreset_ValidatesCorrectly()
        {
            // Arrange
            var validator = new DataValidator();

            // Act & Assert
            Assert.True(validator.Validate("Pass_123!", PresetsValidatorSettings.PasswordSettings));
            Assert.False(validator.Validate("Pass123", PresetsValidatorSettings.PasswordSettings)); // Too short
            Assert.False(validator.Validate("Pass 123!", PresetsValidatorSettings.PasswordSettings)); // Space not allowed
            Assert.True(validator.Validate("Password_123!@#$%^&*()-+=<>?", PresetsValidatorSettings.PasswordSettings));
        }
    }
}
