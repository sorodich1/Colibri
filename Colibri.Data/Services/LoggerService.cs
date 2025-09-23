using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colibri.Data.Services
{
    /// <summary>
    /// Сервис для логирования сообщений.
    /// Реализует интерфейс <see cref="ILoggerService"/>.
    /// </summary>
    public class LoggerService(AppDbContext context) : ILoggerService
    {
        private readonly AppDbContext _context = context;

        /// <summary>
        /// Метод сохраняющий показание телеметрии в БД
        /// </summary>
        /// <param name="telemetry">телеметрия</param>
        /// <exception cref="InvalidOperationException">Ошибка сохранения телеметрии в базе данных</exception>
        public async Task AddTelemetryAsync(Telemetry telemetry)
        {
            try
            {
                _context.Telemetries.Add(telemetry);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Ошибка сохранения телеметрии в базе данных", ex);
            }
        }

        /// <summary>
        /// Записывает сообщение в лог с указанным уровнем логирования и данными о пользователе.
        /// </summary>
        /// <param name="claims">Пользовательские утверждения (<see cref="ClaimsPrincipal"/>) для идентификации пользователя, который инициировал логирование.</param>
        /// <param name="message">Сообщение, которое будет записано в лог.</param>
        /// <param name="logLevel">Уровень логирования, определяющий важность сообщения (<see cref="LogLevel"/>).</param>
        /// <param name="callback">Необязательный коллбэк, который может быть вызван после записи сообщения в лог.</param>
        public void LogMessage(ClaimsPrincipal claims, string message, LogLevel logLevel, Func<Task> callback = null)
        {
            StackTrace stackTrace = new ();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            MethodBase method = stackFrame.GetMethod();

            if (string.IsNullOrEmpty(message))
            {
                message = "";
            }

            Log log = new ()
            {
                Level = logLevel.ToString(),
                Message = message,
                User = claims == null ? "Administrator" : claims.Identity.Name,
                Logger = $"{method.DeclaringType.Name}.{method.Name}"
            };

            _context.Logger.Add(log);
            _context.SaveChanges();
        }
    }
}
