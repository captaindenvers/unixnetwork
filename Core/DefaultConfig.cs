using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UnixLauncher.Core
{
    class DefaultConfig : IConfig
    {
        // Имя файла конфигурации
        public string FileName => "launcher.cfg";

        // Путь к папке, где хранится файл конфигурации
        public string PathToFile => AppDataManager.GetFolder();

        // Полный путь к файлу конфигурации
        public string FullFileName => PathToFile + FileName;

        // Событие, которое генерируется при изменении конфигурации
        public event EventHandler? ConfigChanged;

        // Дефолтные значения для конфигурационного файла
        private readonly List<KeyValuePair<string, string>> _defaultConfigValues =
        [
            // тут будут данные... обязательно будут...
        ];

        // Стартовый комментарий, записываемый в начале файла конфигурации
        private const string START_COMMENT =
            """
            ######################################
            ### UnixNetwork configuration file ###
            ######################################
            #
            # Символ '#' - комментарий, все что здесь находится будет игнорироваться.
            # Лучше тут ничего не изменять, если вы не знаете, что делаете.
            # 
            # Структура <Ключ>=<Значение>
            # Например в записи 'Theme=Dark' - ключу Theme будет присвоено значение Dark


            """;

        /// <summary>
        /// Создаёт файл конфигурации, если он отсутствует или требуется его перезапись.
        /// </summary>
        /// <param name="forceRewriting">Если true, перезаписывает файл даже если он существует.</param>
        private async Task CreateConfig(bool forceRewriting)
        {
            // Создаем директорию, если её нет
            if (!Directory.Exists(PathToFile))
                Directory.CreateDirectory(PathToFile);

            // Если файла нет или требуется перезапись, создаем новый файл и записываем дефолтные значения
            if (!File.Exists(FullFileName) || forceRewriting)
            {
                File.Create(FullFileName).Dispose();

                using StreamWriter streamWriter = new(FullFileName);
                await streamWriter.WriteAsync(START_COMMENT);

                foreach (var cfgField in _defaultConfigValues)
                    await streamWriter.WriteAsync($"{cfgField.Key}={cfgField.Value}\n");
            }
        }

        /// <summary>
        /// Создает или устанавливает свойство конфигурации.
        /// Если файла нет – он создается с дефолтными значениями.
        /// Если ключ уже существует – его значение заменяется, иначе создается новая запись.
        /// </summary>
        /// <typeparam name="T">Тип значения</typeparam>
        /// <param name="key">Ключ конфигурации</param>
        /// <param name="value">Новое значение для ключа</param>
        public async Task CreateOrSetProperty<T>(string key, T value)
        {
            // Если файл конфигурации отсутствует, создаем его
            if (!File.Exists(FullFileName))
            {
                await CreateConfig(forceRewriting: false);
            }

            // Читаем все строки файла для последующего обновления
            List<string> lines = new(await File.ReadAllLinesAsync(FullFileName));
            bool keyFound = false;

            // Проходим по строкам файла, чтобы найти строку с нужным ключом
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string trimmedLine = line.Trim();

                // Пропускаем пустые строки и строки, начинающиеся с комментария
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Убираем inline-комментарии (если есть) для корректного поиска ключа
                int commentIndex = trimmedLine.IndexOf('#');
                string lineContent = commentIndex >= 0 ? trimmedLine.Substring(0, commentIndex).Trim() : trimmedLine;

                // Если строка не содержит '=', пропускаем её
                int equalsIndex = lineContent.IndexOf('=');
                if (equalsIndex < 0)
                    continue;

                // Извлекаем ключ из строки
                string currentKey = lineContent.Substring(0, equalsIndex).Trim();

                // Если ключ совпадает (без учета регистра), обновляем значение
                if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = $"{key}={value}";
                    keyFound = true;
                    break;
                }
            }

            // Если ключ не найден, добавляем новую запись в конец файла
            if (!keyFound)
            {
                lines.Add($"{key}={value}");
            }

            // Перезаписываем файл с обновленными данными
            await File.WriteAllLinesAsync(FullFileName, lines);

            // Уведомляем подписчиков об изменении конфигурации
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Получает значение свойства конфигурации по ключу.
        /// Построчно читается файл с игнорированием комментариев.
        /// </summary>
        /// <param name="key">Искомый ключ</param>
        /// <returns>Если ключ найден – возвращает значение, иначе null</returns>
        public string? GetProperty(string key)
        {
            if (!File.Exists(FullFileName))
                return null;

            foreach (var line in File.ReadLines(FullFileName))
            {
                string trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                int commentIndex = trimmedLine.IndexOf('#');
                if (commentIndex >= 0)
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();

                if (string.IsNullOrEmpty(trimmedLine))
                    continue;

                int equalsIndex = trimmedLine.IndexOf('=');
                if (equalsIndex < 0)
                    continue;

                string currentKey = trimmedLine.Substring(0, equalsIndex).Trim();
                string currentValue = trimmedLine.Substring(equalsIndex + 1).Trim();

                if (string.Equals(currentKey, key, StringComparison.OrdinalIgnoreCase))
                    return currentValue;
            }
            return null;
        }

        /// <summary>
        /// Пытается получить значение свойства и преобразовать его в тип T.
        /// </summary>
        /// <typeparam name="T">Желаемый тип значения</typeparam>
        /// <param name="key">Ключ свойства</param>
        /// <param name="value">Результирующее значение (если преобразование прошло успешно)</param>
        /// <returns>true, если значение получено и успешно преобразовано, иначе false</returns>
        public bool TryGetProperty<T>(string key, out T value)
        {
            value = default!;

            string? propertyValue = GetProperty(key);

            if (propertyValue is null)
                return false;

            try
            {
                if (typeof(T) == typeof(string))
                {
                    value = (T)(object)propertyValue;
                    return true;
                }

                value = (T)Convert.ChangeType(propertyValue, typeof(T));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
