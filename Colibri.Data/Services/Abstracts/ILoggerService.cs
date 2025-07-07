using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colibri.Data.Services.Abstracts
{
    /// <summary>
    /// Интерфейс для сервиса логирования.
    /// Предназначен для записи логов с определенной информацией о пользователе и уровне логирования.
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Записывает сообщение в лог.
        /// </summary>
        /// <param name="claims">Пользовательские утверждения (<see cref="ClaimsPrincipal"/>), связанные с текущим контекстом пользователя.</param>
        /// <param name="message">Сообщение, которое будет записано в лог.</param>
        /// <param name="logLevel">Уровень логирования, определяющий важность сообщения (<see cref="LogLevel"/>).</param>
        /// <param name="callback">Необязательный коллбэк, который будет выполнен после записи сообщения в лог.</param>
        void LogMessage(ClaimsPrincipal claims, string message, LogLevel logLevel, Func<Task> callback = null);
    }
}
