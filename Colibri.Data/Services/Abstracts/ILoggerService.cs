using Colibri.Data.Entity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Записывает телеметрию в БД
        /// </summary>
        /// <param name="telemetry">Сущность телеметрии</param>
        /// <returns>Результат</returns>
        Task AddTelemetryAsync(Telemetry telemetry);

        Task<List<Log>> GetLogsAsync(int page = 1, int pageSize = 50, string level = null, DateTime? fromDate = null, DateTime? toDate = null, string search = null);

        Task<int> GetTotalCountAsync(string level = null, DateTime? fromDate = null, DateTime? toDate = null, string search = null);

        Task<List<string>> GetLogLevelsAsync();

        Task<Log> GetLogByIdAsync(int id);

        Task ClearOldLogsAsync(DateTime olderThan);

        Task DeleteLogsAsync(List<int> logIds);

        Task<bool> DeleteLogAsync(int id);

        Task<List<Log>> GetRecentLogsAsync(int count = 10);

        Task<int> GetUnreadLogsCountAsync(string level = null);
    }
}
