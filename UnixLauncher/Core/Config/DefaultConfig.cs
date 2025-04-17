using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UnixLauncher.Core.Logger;
using UnixLauncher.Core.Providers;

namespace UnixLauncher.Core.Config
{
    class DefaultConfig : IConfig
    {
        // Имя файла конфигурации
        public string FileName { get; private set; } = "launcher.cfg";

        // Путь к папке, где хранится файл конфигурации
        public string PathToFile { get; private set; } = AppDataProvider.GetFolder();

        // Полный путь к файлу конфигурации
        public string FullFileName => Path.Combine(PathToFile, FileName);

        // Событие, которое генерируется при изменении конфигурации
        public event EventHandler? ConfigChanged;

        private readonly ILogger _logger;

        // Объект для блокировки доступа к файлу конфигурации
        private readonly SemaphoreSlim _configFileLock = new SemaphoreSlim(1, 1);

        // Таймаут для операций с семафором (5 секунд)
        private readonly TimeSpan _semaphoreTimeout = TimeSpan.FromSeconds(10);

        // Объект для блокировки доступа к кэшу конфигурации
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        // Кэш конфигурационных значений для уменьшения обращений к файлу
        private readonly Dictionary<string, string> _configCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Дефолтные значения для конфигурационного файла
        private readonly List<(string key, string value, string? comment)> _defaultConfigValues =
        [
            //  Ключ     Значение     Комментарий
            new("Theme", "System",    "Avaible values: Dark, White, System."),
        ];

        /// <summary>
        /// Создает объект с предустановленными, стандартными данными.
        /// <para/>
        /// ( <see cref="FileName"/> = launcher.cfg,
        /// <see cref="PathToFile"/> = <see cref="AppDataProvider.GetFolder"/> )
        /// </summary>
        public DefaultConfig(ILogger logger)
        {
            _logger = logger;
            // Проверка правильности пути
            EnsureValidPath();
            // Загружаем конфигурацию синхронно при создании экземпляра
            Task.Run(() => InitializeConfigAsync()).Wait();
        }

        /// <param name="shouldAddDefaultConfigValues"> 
        ///     Необязательно. Если <b>true</b>, то значения из
        ///     <paramref name="defaultConfigValues"/> ДОБАВЯТСЯ, а не перезапишутся к стандартным.
        /// </param>
        public DefaultConfig(ILogger logger,
                             string fileName,
                             string pathToFile,
                             List<(string key, string value, string? comment)> defaultConfigValues,
                             bool shouldAddDefaultConfigValues = false)
        {
            _logger = logger;
            FileName = fileName;
            PathToFile = pathToFile;

            // Проверка правильности пути
            EnsureValidPath();

            if (shouldAddDefaultConfigValues)
            {
                _defaultConfigValues.AddRange(defaultConfigValues);
            }
            else
            {
                _defaultConfigValues.Clear();
                _defaultConfigValues.AddRange(defaultConfigValues);
            }

            // Загружаем конфигурацию синхронно при создании экземпляра
            Task.Run(() => InitializeConfigAsync()).Wait();
        }

        /// <summary>
        /// Инициализирует конфигурацию при создании экземпляра класса
        /// </summary>
        private async Task InitializeConfigAsync()
        {
            try
            {
                if (!File.Exists(FullFileName))
                {
                    await CreateConfig();
                }
                else
                {
                    await LoadConfigToCacheAsync();
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error initializing config", ex);
            }
        }

        // Проверяет и корректирует путь к файлу
        private void EnsureValidPath()
        {
            if (!string.IsNullOrEmpty(PathToFile) && !PathToFile.EndsWith(Path.DirectorySeparatorChar))
            {
                PathToFile += Path.DirectorySeparatorChar;
            }
        }

        // Стартовый комментарий, записываемый в начале файла конфигурации
        private const string START_COMMENT =
            """
            ######################################
            ### UnixNetwork configuration file ###
            ######################################
            #
            # Комментарии.
            # Символ '#' - комментарий. Все что находится после этого символа будет игнорироваться.
            # Комментарий начинается ТОЛЬКО с начала строки. Если он будет находиться в ключе/значении, это будет считать его частью.
            # Комментарии должны быть расположены над парой ключ-значение, включать в себя вариации значения.
            # 
            # Общие положения.
            # Структура <Ключ>=<Значение>
            # Например в записи 'Theme=Dark' - ключу Theme будет присвоено значение Dark
            #
            # Регистр ключа не имеет значения. THEME, ThEmE, theme - будет считаться одним и тем же.
            # Регистр значения ИМЕЕТ значение. Dark, DARK, dark, DaRk - разные значения.
            #
            # При наличии нескольких одинаковых ключей, будет использован первый найденный.
            # 
            # Лучше тут ничего не изменять, если вы не знаете, что делаете.
            # Конфиг ниже
            #  ↓  ↓  ↓


            """;

        public string GetFileName() => FileName;

        public string GetPathToFile() => PathToFile;

        /// <summary>
        /// Создаёт файл конфигурации, если он отсутствует или требуется его перезапись.
        /// </summary>
        /// <param name="forceRewriting">Если true, перезаписывает файл даже если он существует.</param>
        private async Task CreateConfig(bool forceRewriting = false)
        {
            // Используем тайм-аут для предотвращения бесконечного ожидания
            bool lockTaken = await _configFileLock.WaitAsync(_semaphoreTimeout);
            if (!lockTaken)
            {
                await _logger.ErrorAsync("Failed to acquire lock for creating config file - timeout.");
                throw new TimeoutException("Failed to acquire lock for creating config file.");
            }

            try
            {
                // Создаем директорию, если её нет
                if (!Directory.Exists(PathToFile))
                {
                    Directory.CreateDirectory(PathToFile);
                }

                // Если файла нет или требуется перезапись, создаем новый файл и записываем дефолтные значения
                if (!File.Exists(FullFileName) || forceRewriting)
                {
                    using StreamWriter streamWriter = new StreamWriter(FullFileName, false);
                    await streamWriter.WriteAsync(START_COMMENT);

                    foreach (var cfgField in _defaultConfigValues)
                    {
                        string commentLine = string.IsNullOrEmpty(cfgField.comment) ? "" : $"#{cfgField.comment}\n";
                        await streamWriter.WriteAsync($"{commentLine}{cfgField.key}={cfgField.value}\n\n");

                        // Кэшируем дефолтные значения
                        _cacheLock.EnterWriteLock();
                        try
                        {
                            _configCache[cfgField.key] = cfgField.value;
                        }
                        finally
                        {
                            _cacheLock.ExitWriteLock();
                        }
                    }

                    await _logger.DebugAsync("Created default config at " + FullFileName);
                }
                else
                {
                    // Если файл существует и мы не перезаписываем его, загружаем значения в кэш
                    await LoadConfigToCacheAsync();
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error creating config file", ex);
                throw; // Пробрасываем исключение для обработки на более высоком уровне
            }
            finally
            {
                _configFileLock.Release();
            }
        }

        /// <summary>
        /// Загружает все конфигурационные значения из файла в кэш
        /// </summary>
        private async Task LoadConfigToCacheAsync()
        {
            if (!File.Exists(FullFileName))
                return;

            string[] lines;
            try
            {
                lines = await File.ReadAllLinesAsync(FullFileName);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error reading config file", ex);
                return;
            }

            _cacheLock.EnterWriteLock();
            try
            {
                _configCache.Clear();

                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                        continue;

                    int equalsIndex = trimmedLine.IndexOf('=');
                    if (equalsIndex < 0)
                        continue;

                    string currentKey = trimmedLine.Substring(0, equalsIndex).Trim();
                    string currentValue = trimmedLine.Substring(equalsIndex + 1).Trim();

                    if (!string.IsNullOrWhiteSpace(currentValue) && !_configCache.ContainsKey(currentKey))
                        _configCache[currentKey] = currentValue;
                }
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Создает или устанавливает свойство конфигурации.
        /// Если файла нет – он создается с дефолтными значениями.
        /// Если ключ уже существует – его значение заменяется, иначе создается новая запись.
        /// <throws>Выбрасывает <see cref="ArgumentException"/>, если передан пустой ключ</throws>
        /// </summary>
        public async Task CreateOrSetProperty<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            // Преобразуем значение в строку
            string stringValue = value?.ToString() ?? string.Empty;

            // Обновление кэша
            _cacheLock.EnterWriteLock();
            try
            {
                _configCache[key] = stringValue;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }

            // Используем тайм-аут для предотвращения бесконечного ожидания
            bool lockTaken = await _configFileLock.WaitAsync(_semaphoreTimeout);
            if (!lockTaken)
            {
                await _logger.ErrorAsync("Failed to acquire lock for updating config - timeout.");
                throw new TimeoutException("Failed to acquire lock for updating config.");
            }

            try
            {
                // Если файл конфигурации отсутствует, создаем его
                if (!File.Exists(FullFileName))
                {
                    await _logger.DebugAsync($"{FullFileName} does not exist, creating...");
                    await CreateConfig();
                }

                List<string> lines;
                try
                {
                    // Читаем все строки файла для последующего обновления
                    lines = new List<string>(await File.ReadAllLinesAsync(FullFileName));
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Error reading config file during update", ex);
                    lines = new List<string>(); // Если не удалось прочитать, создаем пустой список
                }

                bool keyFound = false;

                // Проходим по строкам файла, чтобы найти строку с нужным ключом
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    string lineContent = line.Trim();

                    // Пропускаем пустые строки и строки, начинающиеся с комментария
                    if (string.IsNullOrEmpty(lineContent) || lineContent.StartsWith('#'))
                        continue;

                    // Если строка не содержит '=', пропускаем её
                    int equalsIndex = lineContent.IndexOf('=');
                    if (equalsIndex <= 0)
                        continue;

                    // Извлекаем ключ из строки
                    string currentKey = lineContent.Substring(0, equalsIndex).Trim();

                    // Если ключ совпадает (без учета регистра), обновляем значение
                    if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"{key}={stringValue}";
                        keyFound = true;
                        break;
                    }
                }

                // Если ключ не найден, добавляем новую запись в конец файла
                if (!keyFound)
                {
                    lines.Add($"{key}={stringValue}");
                }

                try
                {
                    // Перезаписываем файл с обновленными данными
                    await File.WriteAllLinesAsync(FullFileName, lines);
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Error writing to config file", ex);
                    throw; // Пробрасываем исключение для обработки на более высоком уровне
                }

                // Уведомляем подписчиков об изменении конфигурации
                OnConfigChanged();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error updating config for key: {key}", ex);
                throw;
            }
            finally
            {
                // Всегда освобождаем семафор, даже при исключении
                _configFileLock.Release();
            }
        }

        private void OnConfigChanged()
        {
            // Делаем локальную копию делегата, чтобы избежать состояния гонки
            var handler = ConfigChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Получает значение свойства конфигурации из кэша без обращения к файлу. 
        /// <throws>Выбрасывает <see cref="ArgumentException"/>, если передан пустой ключ. </throws>
        /// </summary>
        /// <returns>Значение из кэша или пустая строка</returns>
        private string GetPropertyFromCache(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key can't be empty!", nameof(key));

            _cacheLock.EnterReadLock();
            try
            {
                if (_configCache.TryGetValue(key, out var cachedValue))
                    return cachedValue;
                return string.Empty;
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Получает значение свойства конфигурации по ключу.
        /// Сначала проверяет значение в кэше, затем в файле.
        /// Выбрасывает <see cref="ArgumentException"/>, если переданный ключ оказался пустым
        /// </summary>
        /// <param name="key">Искомый ключ</param>
        /// <returns>Если ключ найден – возвращает значение, иначе пустую строку</returns>
        public async Task<string> GetPropertyAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key can't be empty!", nameof(key));

            // Сначала проверяем значение в кэше
            string cachedValue = GetPropertyFromCache(key);
            if (!string.IsNullOrEmpty(cachedValue))
                return cachedValue;

            // Используем тайм-аут для предотвращения бесконечного ожидания
            bool lockTaken = await _configFileLock.WaitAsync(_semaphoreTimeout);
            if (!lockTaken)
            {
                await _logger.ErrorAsync("Failed to acquire lock for reading config - timeout.");
                // В случае таймаута возвращаем пустую строку, чтобы не блокировать выполнение
                return string.Empty;
            }

            try
            {
                // Перепроверяем кэш после получения блокировки
                cachedValue = GetPropertyFromCache(key);
                if (!string.IsNullOrEmpty(cachedValue))
                    return cachedValue;

                if (!File.Exists(FullFileName))
                {
                    await CreateConfig();
                    return GetPropertyFromCache(key); // После создания снова проверяем кэш
                }

                string result = string.Empty;

                try
                {
                    foreach (var line in await File.ReadAllLinesAsync(FullFileName))
                    {
                        string trimmedLine = line.Trim();
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                            continue;

                        int equalsIndex = trimmedLine.IndexOf('=');
                        if (equalsIndex < 0)
                            continue;

                        string currentKey = trimmedLine.Substring(0, equalsIndex).Trim();
                        string currentValue = trimmedLine.Substring(equalsIndex + 1).Trim();

                        if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrWhiteSpace(currentValue))
                        {
                            result = currentValue;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync("Error reading config file", ex);
                    return string.Empty;
                }

                // Обновляем кэш найденным значением
                if (!string.IsNullOrEmpty(result))
                {
                    _cacheLock.EnterWriteLock();
                    try
                    {
                        _configCache[key] = result;
                    }
                    finally
                    {
                        _cacheLock.ExitWriteLock();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"Error getting property: {key}", ex);
                return string.Empty;
            }
            finally
            {
                // Всегда освобождаем семафор, даже при исключении
                _configFileLock.Release();
            }
        }

        /// <summary>
        /// Пытается получить значение свойства и преобразовать его в тип T.
        /// </summary>
        /// <typeparam name="T">Желаемый тип значения</typeparam>
        /// <param name="key">Ключ свойства</param>
        /// <param name="value">Результирующее значение (если преобразование прошло успешно)</param>
        /// <returns>true, если значение получено и успешно преобразовано, иначе false</returns>
        public bool TryGetProperty<T>(string key, out T? value)
        {
            value = default;

            try
            {
                // Сначала проверяем значение в кэше (синхронно)
                string cachedValue = GetPropertyFromCache(key);

                // Если значение не найдено в кэше, получаем его из файла
                if (string.IsNullOrEmpty(cachedValue))
                {
                    try
                    {
                        // Используем таймаут для предотвращения бесконечного ожидания
                        var task = Task.Run(() => GetPropertyAsync(key));

                        // Ждем результат не более 5 секунд
                        if (task.Wait(_semaphoreTimeout))
                        {
                            cachedValue = task.Result;
                        }
                        else
                        {
                            return false; // Таймаут при получении значения
                        }
                    }
                    catch (Exception ex)
                    {
                        Task.Run(() => _logger.ErrorAsync("Error getting property async", ex)).Wait(_semaphoreTimeout);
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(cachedValue))
                    return false;

                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)cachedValue;
                    return true;
                }

                value = (T)Convert.ChangeType(cachedValue, typeof(T));
                return true;
            }
            catch (Exception e)
            {
                Task.Run(() => _logger.ErrorAsync("TYPE CONVERSION ERROR", e)).Wait(_semaphoreTimeout);
                return false;
            }
        }

        /// <summary>
        /// Освобождает ресурсы, используемые классом
        /// </summary>
        public void Dispose()
        {
            _configFileLock.Dispose();
            _cacheLock.Dispose();
        }
    }
}