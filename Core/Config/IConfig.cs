namespace UnixLauncher.Core.Config
{
    interface IConfig
    {
        /// <summary>
        /// Gets the name of the configuration file with extention.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the full path to the configuration file | NOT including file name.
        /// </summary>
        string PathToFile { get; }

        /// <summary>
        /// Literally PathToFile + FileName
        /// </summary>
        public string FullFileName { get; }

        /// <summary>
        /// Creates the field if it doesn't exist, and replaces the value if it does.
        /// </summary>
        Task CreateOrSetProperty<T>(string key, T value);

        /// <summary>
        /// Retrieves a configuration value as a string.
        /// Returns null if the key does not exist.
        /// </summary>
        /// <returns>The corresponding value, or null if not found.</returns>
        string? GetProperty(string key);

        /// <summary>
        /// Attempts to retrieve a configuration value with type safety.
        /// </summary>
        /// <typeparam name="T">The expected type of the value.</typeparam>
        /// <param name="value">The output parameter containing the retrieved value if found.</param>
        /// <returns>True if the value exists and is of the correct type; otherwise, false.</returns>
        bool TryGetProperty<T>(string key, out T value);

        /// <summary>
        /// Raised when the configuration settings change.
        /// </summary>
        event EventHandler? ConfigChanged;
    }
}
