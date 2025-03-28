using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnixLauncher.Core.Logger
{
    /// <summary>
    /// Определяет контракт для логгера, позволяющий записывать сообщения различных уровней важности и управлять областью логирования.
    /// </summary>
    /// <remarks>
    /// Он обеспечивает методы для асинхронной записи логов с уровнями: Trace, Debug, Info, Warn и Error, а также поддержку контекста логирования.
    /// </remarks>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Проверяет, включено ли логирование для указанного уровня.
        /// </summary>
        /// <param name="level">Уровень логирования.</param>
        /// <returns>
        /// <c>true</c>, если логирование для заданного уровня включено; иначе, <c>false</c>.
        /// </returns>
        bool IsEnabled(LogLevel level);

        /// <summary>
        /// Начинает область логирования с указанным состоянием.
        /// </summary>
        /// <typeparam name="TState">Тип состояния, ассоциированного с областью логирования.</typeparam>
        /// <param name="state">Состояние, которое будет ассоциировано с областью логирования.</param>
        /// <returns>
        /// Объект <see cref="IDisposable"/>, вызов метода <see cref="IDisposable.Dispose"/> которого завершит текущую область логирования.
        /// </returns>
        IDisposable BeginScope<TState>(TState state);

        /// <summary>
        /// Асинхронно записывает трассировочное сообщение.
        /// </summary>
        /// <param name="message">Сообщение для логирования.</param>
        Task TraceAsync(string message);

        /// <summary>
        /// Асинхронно записывает отладочное сообщение.
        /// </summary>
        /// <param name="message">Сообщение для логирования.</param>
        Task DebugAsync(string message);

        /// <summary>
        /// Асинхронно записывает информационное сообщение.
        /// </summary>
        /// <param name="message">Сообщение для логирования.</param>
        Task InfoAsync(string message);

        /// <summary>
        /// Асинхронно записывает предупреждающее сообщение.
        /// </summary>
        /// <param name="message">Сообщение для логирования.</param>
        Task WarnAsync(string message);

        /// <summary>
        /// Асинхронно записывает сообщение об ошибке.
        /// </summary>
        /// <param name="message">Сообщение об ошибке для логирования.</param>
        Task ErrorAsync(string message);

        /// <summary>
        /// Асинхронно записывает сообщение об ошибке вместе с информацией об исключении.
        /// </summary>
        /// <param name="message">Сообщение об ошибке для логирования.</param>
        /// <param name="exception">Исключение, связанное с ошибкой.</param>
        Task ErrorAsync(string message, Exception exception);

        /// <summary>
        /// Асинхронно записывает информацию об исключении.
        /// </summary>
        /// <param name="exception">Исключение, которое необходимо залогировать.</param>
        Task ErrorAsync(Exception exception);
    }

}
