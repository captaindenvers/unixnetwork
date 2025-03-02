using Xunit;
using Moq;
using FluentAssertions;
using UnixLauncher.Core;
using UnixLauncher.Core.Config;

namespace UnixLauncher.Tests
{
    public class DefaultConfigTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testFileName;
        private readonly string _fullTestPath;
        private readonly List<KeyValuePair<string, string>> _testDefaultValues;
        private IConfig _config;

        public DefaultConfigTests()
        {
            // Setup test environment
            _testDirectory = Path.Combine(Path.GetTempPath(), "UnixLauncherTests", Guid.NewGuid().ToString());
            _testFileName = "test_launcher.cfg";
            Directory.CreateDirectory(_testDirectory);
            _fullTestPath = Path.Combine(_testDirectory, _testFileName);

            _testDefaultValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Theme", "Dark"),
                new KeyValuePair<string, string>("Language", "English")
            };

            _config = new DefaultConfig(_testFileName, _testDirectory + Path.DirectorySeparatorChar, _testDefaultValues);
        }

        public void Dispose()
        {
            // Clean up test environment
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void Constructor_SetsCorrectProperties()
        {
            // Act & Assert
            _config.GetFileName().Should().Be(_testFileName);
            _config.GetPathToFile().Should().Be(_testDirectory + Path.DirectorySeparatorChar);
            _config.FullFileName.Should().Be(_fullTestPath);
        }

        [Fact]
        public void DefaultConstructor_SetsDefaultValues()
        {
            // Arrange
            var defaultConfig = new DefaultConfig();

            // Act & Assert
            defaultConfig.GetFileName().Should().Be("launcher.cfg");
            defaultConfig.GetPathToFile().Should().Be(AppDataManager.GetFolder());
        }

        [Fact]
        public async Task CreateOrSetProperty_CreatesConfigFile_WhenFileDoesNotExist()
        {
            // Act
            await _config.CreateOrSetProperty("TestKey", "TestValue");

            // Assert
            File.Exists(_fullTestPath).Should().BeTrue();
            var fileContent = await File.ReadAllTextAsync(_fullTestPath);
            fileContent.Should().Contain("TestKey=TestValue");
            fileContent.Should().Contain("UnixNetwork configuration file");
        }

        [Fact]
        public async Task CreateOrSetProperty_AddsDefaultValues_WhenCreatingNewFile()
        {
            // Act
            await _config.CreateOrSetProperty("NewKey", "NewValue");

            // Assert
            var fileContent = await File.ReadAllTextAsync(_fullTestPath);
            fileContent.Should().Contain("Theme=Dark");
            fileContent.Should().Contain("Language=English");
            fileContent.Should().Contain("NewKey=NewValue");
        }

        [Fact]
        public async Task CreateOrSetProperty_UpdatesExistingKey()
        {
            // Arrange
            await _config.CreateOrSetProperty("KeyToUpdate", "InitialValue");

            // Act
            await _config.CreateOrSetProperty("KeyToUpdate", "UpdatedValue");

            // Assert
            var fileContent = await File.ReadAllTextAsync(_fullTestPath);
            fileContent.Should().Contain("KeyToUpdate=UpdatedValue");
            fileContent.Should().NotContain("KeyToUpdate=InitialValue");
        }

        [Fact]
        public async Task CreateOrSetProperty_TriggersConfigChangedEvent()
        {
            // Arrange
            bool eventTriggered = false;
            _config.ConfigChanged += (s, e) => eventTriggered = true;

            // Act
            await _config.CreateOrSetProperty("TestKey", "TestValue");

            // Assert
            eventTriggered.Should().BeTrue();
        }

        [Fact]
        public async Task GetProperty_ReturnsNull_WhenFileDoesNotExist()
        {
            // Arrange - ensure file doesn't exist
            if (File.Exists(_fullTestPath))
                File.Delete(_fullTestPath);

            // Act
            string? result = _config.GetProperty("AnyKey");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProperty_ReturnsValue_WhenKeyExists()
        {
            // Arrange
            await _config.CreateOrSetProperty("ExistingKey", "ExistingValue");

            // Act
            string? result = _config.GetProperty("ExistingKey");

            // Assert
            result.Should().Be("ExistingValue");
        }

        [Fact]
        public async Task GetProperty_ReturnsNull_WhenKeyDoesNotExist()
        {
            // Arrange
            await _config.CreateOrSetProperty("SomeKey", "SomeValue");

            // Act
            string? result = _config.GetProperty("NonExistentKey");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetProperty_IgnoresComments_InConfigFile()
        {
            // Arrange - create a file with comments manually
            string fileContent =
                "# This is a comment\n" +
                "Key1=Value1 # This is an inline comment\n" +
                "# Key2=Value2 This key is commented out\n" +
                "Key3=Value3";

            Directory.CreateDirectory(_testDirectory);
            await File.WriteAllTextAsync(_fullTestPath, fileContent);

            // Act
            string? result1 = _config.GetProperty("Key1");
            string? result2 = _config.GetProperty("Key2");
            string? result3 = _config.GetProperty("Key3");

            // Assert
            result1.Should().Be("Value1");
            result2.Should().BeNull();
            result3.Should().Be("Value3");
        }

        [Fact]
        public async Task GetProperty_HandlesEmptyLines_InConfigFile()
        {
            // Arrange
            string fileContent =
                "Key1=Value1\n" +
                "\n" +
                "\n" +
                "Key2=Value2";

            Directory.CreateDirectory(_testDirectory);
            await File.WriteAllTextAsync(_fullTestPath, fileContent);

            // Act
            string? result1 = _config.GetProperty("Key1");
            string? result2 = _config.GetProperty("Key2");

            // Assert
            result1.Should().Be("Value1");
            result2.Should().Be("Value2");
        }

        [Fact]
        public async Task TryGetProperty_ReturnsFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            await _config.CreateOrSetProperty("ExistingKey", "ExistingValue");

            // Act
            bool result = _config.TryGetProperty("NonExistentKey", out string value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public async Task TryGetProperty_ReturnsFalse_WhenConversionFails()
        {
            // Arrange
            await _config.CreateOrSetProperty("StringKey", "NotAnInteger");

            // Act
            bool result = _config.TryGetProperty("StringKey", out int value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(0); // default value for int
        }

        [Fact]
        public async Task TryGetProperty_ReturnsTrue_WithCorrectValue_WhenSuccessful()
        {
            // Arrange
            await _config.CreateOrSetProperty("IntKey", "42");
            await _config.CreateOrSetProperty("BoolKey", "true");
            await _config.CreateOrSetProperty("StringKey", "Hello");

            // Act
            bool intResult = _config.TryGetProperty("IntKey", out int intValue);
            bool boolResult = _config.TryGetProperty("BoolKey", out bool boolValue);
            bool stringResult = _config.TryGetProperty("StringKey", out string stringValue);

            // Assert
            intResult.Should().BeTrue();
            intValue.Should().Be(42);

            boolResult.Should().BeTrue();
            boolValue.Should().BeTrue();

            stringResult.Should().BeTrue();
            stringValue.Should().Be("Hello");
        }

        [Fact]
        public async Task CreateOrSetProperty_HandlesSpecialCases()
        {
            // Arrange & Act
            await _config.CreateOrSetProperty("KeyWithEquals", "Value=WithEquals");
            await _config.CreateOrSetProperty("KeyWithHash", "Value#WithHash");
            await _config.CreateOrSetProperty("EmptyValue", "");

            // Assert
            _config.GetProperty("KeyWithEquals").Should().Be("Value=WithEquals");
            _config.GetProperty("KeyWithHash").Should().Be("Value");
            _config.GetProperty("EmptyValue").Should().BeNull();
        }

        [Fact]
        public async Task CreateOrSetProperty_PreservesOtherKeys_WhenUpdating()
        {
            // Arrange
            await _config.CreateOrSetProperty("Key1", "Value1");
            await _config.CreateOrSetProperty("Key2", "Value2");

            // Act
            await _config.CreateOrSetProperty("Key1", "UpdatedValue");

            // Assert
            _config.GetProperty("Key1").Should().Be("UpdatedValue");
            _config.GetProperty("Key2").Should().Be("Value2");
        }

        [Fact]
        public async Task GetProperty_IsCaseInsensitive()
        {
            // Arrange
            await _config.CreateOrSetProperty("CaseSensitiveKey", "Value");

            // Act
            string? result1 = _config.GetProperty("casesensitivekey");
            string? result2 = _config.GetProperty("CASESENSITIVEKEY");
            string? result3 = _config.GetProperty("CaseSensitiveKey");

            // Assert
            result1.Should().Be("Value");
            result2.Should().Be("Value");
            result3.Should().Be("Value");
        }

        [Fact]
        public async Task CreateOrSetProperty_HandlesKeyWithCommentCharacters()
        {
            // Arrange & Act
            await _config.CreateOrSetProperty("Key#WithHash", "Value");

            // Assert
            _config.GetProperty("Key#WithHash").Should().BeNull();
        }

        [Fact]
        public async Task GetProperty_HandlesComplexCommentScenarios()
        {
            // Arrange - create a file with tricky comments
            string fileContent =
                "SimpleKey=SimpleValue\n" +
                "TrickyKey=Value # With # Multiple # Hashes\n" +
                "# Comment=NotAValue\n" +
                "Key=Value####NotValue\n" +
                "EmptyComment=#\n" +
                "HashPrefix=#Value";

            Directory.CreateDirectory(_testDirectory);
            await File.WriteAllTextAsync(_fullTestPath, fileContent);

            // Act & Assert
            _config.GetProperty("SimpleKey").Should().Be("SimpleValue");
            _config.GetProperty("TrickyKey").Should().Be("Value");
            _config.GetProperty("Comment").Should().BeNull();
            _config.GetProperty("Key").Should().Be("Value");
            _config.GetProperty("EmptyComment").Should().BeNull();
            _config.GetProperty("HashPrefix").Should().BeNull();
        }

        [Fact]
        public async Task CreateOrSetProperty_MaintainsFileIntegrity()
        {
            // Arrange
            await _config.CreateOrSetProperty("Key1", "Value1");
            string initialContent = await File.ReadAllTextAsync(_fullTestPath);

            // Act
            await _config.CreateOrSetProperty("Key2", "Value2");
            string updatedContent = await File.ReadAllTextAsync(_fullTestPath);

            // Assert
            updatedContent.Should().Contain(initialContent);
            updatedContent.Should().Contain("Key2=Value2");
        }

        [Fact]
        public async Task CreateOrSetProperty_HandlesValuesWithSpecialCharacters()
        {
            // Arrange & Act
            await _config.CreateOrSetProperty("SpecialKey", "Value with spaces, commas, and symbols: !@#$%^&*()");

            // Assert
            _config.GetProperty("SpecialKey").Should().Be("Value with spaces, commas, and symbols: !@");
        }

        [Fact]
        public async Task GetProperty_HandlesInvalidLines()
        {
            // Arrange - file with invalid lines
            string fileContent =
                "ValidKey=ValidValue\n" +
                "InvalidLine-NoEquals\n" +
                "=ValueWithoutKey\n" +
                "KeyWithoutValue=";

            Directory.CreateDirectory(_testDirectory);
            await File.WriteAllTextAsync(_fullTestPath, fileContent);

            // Act & Assert
            _config.GetProperty("ValidKey").Should().Be("ValidValue");
            _config.GetProperty("InvalidLine-NoEquals").Should().BeNull();
            _config.GetProperty("").Should().BeNull();
            _config.GetProperty("KeyWithoutValue").Should().BeNull();
        }
    }
}