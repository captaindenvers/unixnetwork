using FluentAssertions;
using System.Runtime.CompilerServices;
using UnixLauncher.Core.Config;
using UnixLauncher.Core.Logger;

namespace UnixLauncher.Tests
{
    public class DefaultConfigTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _testFileName;
        private readonly string _fullTestPath;
        private readonly List<(string key, string value, string? comment)> _testDefaultValues;
        private IConfig _config;
        private ILogger _defaultLogger;

        public DefaultConfigTests()
        {
            // Setup test environment
            _testDirectory = Path.Combine(Path.GetTempPath(), "UnixLauncherTests", Guid.NewGuid().ToString());
            //_testDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "UnixLauncherTests", Guid.NewGuid().ToString());
            _testFileName = "test_launcher.cfg";
            Directory.CreateDirectory(_testDirectory);
            _fullTestPath = Path.Combine(_testDirectory, _testFileName);

            LoggerOptions loggerOptions = new()
            {
                FileName = "TestLogs.txt",
                Directory = _testDirectory,
                MinLogLevel = LogLevel.Trace,
            };

            _defaultLogger = new FileLogger(loggerOptions);

            _testDefaultValues = new List<(string, string, string?)>
            {
                new("Theme", "Dark", "idk, test??"),
                new("Language", "English", "another test???")
            };

            _config = new DefaultConfig(_defaultLogger,
                                        _testFileName,
                                        _testDirectory + Path.DirectorySeparatorChar,
                                        _testDefaultValues);
        }

        public void Dispose()
        {
            _config.Dispose();

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
        public async Task GetPropertyAsync_ReturnsEmpty_WhenFileDoesNotExist()
        {
            // Arrange - ensure file doesn't exist
            if (File.Exists(_fullTestPath))
                File.Delete(_fullTestPath);

            // Act
            string? result = await _config.GetPropertyAsync("AnyKey");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPropertyAsync_ReturnsValue_WhenKeyExists()
        {
            // Arrange
            await _config.CreateOrSetProperty("ExistingKey", "ExistingValue");

            // Act
            string? result = await _config.GetPropertyAsync("ExistingKey");

            // Assert
            result.Should().Be("ExistingValue");
        }

        [Fact]
        public async Task GetPropertyAsync_ReturnsNull_WhenKeyDoesNotExist()
        {
            // Arrange
            await _config.CreateOrSetProperty("SomeKey", "SomeValue");

            // Act
            string? result = await _config.GetPropertyAsync("NonExistentKey");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPropertyAsync_IgnoresComments_InConfigFile()
        {
            // Arrange - create a file with comments manually
            string fileContent =
                "# This is a comment\n" +
                "Key1=Value1 # This is NOT a comment\n" +
                "# Key2=Value2 This key is commented out\n" +
                "Key3=Value3";

            Directory.CreateDirectory(_testDirectory);
            await File.WriteAllTextAsync(_fullTestPath, fileContent);

            // Act
            string? result1 = await _config.GetPropertyAsync("Key1");
            string? result2 = await _config.GetPropertyAsync("Key2");
            string? result3 = await _config.GetPropertyAsync("Key3");

            // Assert
            result1.Should().Be("Value1 # This is NOT a comment");
            result2.Should().BeEmpty();
            result3.Should().Be("Value3");
        }

        [Fact]
        public async Task GetPropertyAsync_HandlesEmptyLines_InConfigFile()
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
            string? result1 = await _config.GetPropertyAsync("Key1");
            string? result2 = await _config.GetPropertyAsync("Key2");

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
            bool result = _config.TryGetProperty("NonExistentKey", out string? value);

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
            bool stringResult = _config.TryGetProperty("StringKey", out string? stringValue);

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
            string? t1 = await _config.GetPropertyAsync("KeyWithEquals");
            t1.Should().Be("Value=WithEquals");

            string? t2 = await _config.GetPropertyAsync("KeyWithHash");
            t2.Should().Be("Value#WithHash"); // Value#WithHash а не просто Value

            string? t3 = await _config.GetPropertyAsync("EmptyValue");
            t3.Should().Be(""); // Пустая строка, а не null
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
            string? t1 = await _config.GetPropertyAsync("Key1");
            t1.Should().Be("UpdatedValue");

            string? t2 = await _config.GetPropertyAsync("Key2");
            t2.Should().Be("Value2");
        }

        [Fact]
        public async Task GetPropertyAsync_IsCaseInsensitive()
        {
            // Arrange
            await _config.CreateOrSetProperty("CaseSensitiveKey", "Value");

            // Act
            string? result1 = await _config.GetPropertyAsync("casesensitivekey");
            string? result2 = await _config.GetPropertyAsync("CASESENSITIVEKEY");
            string? result3 = await _config.GetPropertyAsync("CaseSensitiveKey");

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
            string? result = await _config.GetPropertyAsync("Key");
            result.Should().BeEmpty();

            string? result2 = await _config.GetPropertyAsync("Key#WithHash");
            result2.Should().Be("Value");
        }

        [Fact]
        public async Task GetPropertyAsync_HandlesComplexCommentScenarios()
        {
            // Arrange - create a file with tricky comments
            string fileContent =
                "SimpleKey=SimpleValue\n" +
                "TrickyKey=Value # With # Multiple # Hashes\n" +
                "# Comment=NotAValue\n" +
                "Key=Value####StillValue\n" +
                "EmptyComment=#\n" +
                "HashPrefix=#Value";

            Directory.CreateDirectory(_testDirectory);
            await File.WriteAllTextAsync(_fullTestPath, fileContent);

            // Act & Assert
            string? simpleKey = await _config.GetPropertyAsync("SimpleKey");
            simpleKey.Should().Be("SimpleValue");

            string? trickyKey = await _config.GetPropertyAsync("TrickyKey");
            trickyKey.Should().Be("Value # With # Multiple # Hashes");

            string? comment = await _config.GetPropertyAsync("Comment");
            comment.Should().BeEmpty();

            string? key = await _config.GetPropertyAsync("Key");
            key.Should().Be("Value####StillValue");

            string? emptyComment = await _config.GetPropertyAsync("EmptyComment");
            emptyComment.Should().Be("#");

            string? hashPrefix = await _config.GetPropertyAsync("HashPrefix");
            hashPrefix.Should().Be("#Value");
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
            string specialValue = "Value with spaces, commas, and symbols: !@#$%^&*()";
            await _config.CreateOrSetProperty("SpecialKey", specialValue);

            // Assert
            string? result = await _config.GetPropertyAsync("SpecialKey");
            result.Should().Be(specialValue); // Должно вернуть полное значение, а не обрезанное
        }

        [Fact]
        public async Task GetPropertyAsync_HandlesInvalidLines()
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
            string validKey = await _config.GetPropertyAsync("ValidKey");
            validKey.Should().Be("ValidValue");

            string invalidLine = await _config.GetPropertyAsync("InvalidLine-NoEquals");
            invalidLine.Should().BeEmpty();

            await Assert.ThrowsAsync<ArgumentException>(async () => await _config.GetPropertyAsync(""));

            string keyWithoutValue = await _config.GetPropertyAsync("KeyWithoutValue");
            keyWithoutValue.Should().BeEmpty(); // Пустая строка, а не null
        }
    }
}