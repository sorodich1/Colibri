using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public async Task ClearOldLogsAsync(DateTime olderThan)
        {
            var oldLogs = await _context.Logger
                .Where(l => l.Timestamp < olderThan)
                .ToListAsync();

            _context.Logger.RemoveRange(oldLogs);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteLogAsync(int id)
        {
            var log = await _context.Logger.FindAsync(id);
            if (log != null)
            {
                _context.Logger.Remove(log);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task DeleteLogsAsync(List<int> logIds)
        {
            if (logIds == null || logIds.Count == 0)
                return;

            var logsToDelete = await _context.Logger
                .Where(l => logIds.Contains(l.Id))
                .ToListAsync();
            
            if (logsToDelete.Count > 0)
            {
                _context.Logger.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Log> GetLogByIdAsync(int id)
        {
            return await _context.Logger
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<List<string>> GetLogLevelsAsync()
        {
            return await _context.Logger
                .Where(l => l.Level != null)
                .Select(l => l.Level!)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }

        public async Task<List<Log>> GetLogsAsync(int page = 1, int pageSize = 50, string level = null, DateTime? fromDate = null, DateTime? toDate = null, string search = null)
        {
            var query = _context.Logger.AsQueryable();

            // Применяем фильтры
            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => 
                    l.Message != null && l.Message.Contains(search) ||
                    l.User != null && l.User.Contains(search) ||
                    l.Logger != null && l.Logger.Contains(search));
            }

            // Сортировка и пагинация
            return await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Log>> GetRecentLogsAsync(int count = 10)
        {
            return await _context.Logger
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string level = null, DateTime? fromDate = null, DateTime? toDate = null, string search = null)
        {
            var query = _context.Logger.AsQueryable();

            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= toDate.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => 
                    l.Message != null && l.Message.Contains(search) ||
                    l.User != null && l.User.Contains(search) ||
                    l.Logger != null && l.Logger.Contains(search));
            }

            return await query.CountAsync();
        }

        public async Task<int> GetUnreadLogsCountAsync(string level = null)
        {
            var query = _context.Logger.AsQueryable();
            
            if (!string.IsNullOrEmpty(level))
            {
                query = query.Where(l => l.Level == level);
            }
            
            return await query.CountAsync();
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
