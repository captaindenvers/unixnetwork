namespace UnixLauncher.Core.Validator
{
    internal class DataValidator : IDataValidator
    {
        /// <summary>
        /// Подтверждает данные, введенные пользователем
        /// Запрещает превышение доступных символов и ввод спец. символов по настройкам.
        /// Проверка по длины строгая (< / >).
        /// </summary>
        /// <returns>Возвращает true, если данные прошли проверку, false в обратном случае</returns>
        public bool Validate(string text, ValidatorSettings validatorSettings)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            // Проверка длины
            if (text.Length > validatorSettings.MaxLength || text.Length < validatorSettings.MinLength)
                return false;

            // Поиск несоответствующих
            if (!text.All(c => validatorSettings.AllowedSymbols.Contains(c)))
                return false;

            return true;
        }
    }
}
