using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Colibri.WebApi.Controllers
{
    [Route("logs")]
    public class LogsController : Controller
    {
        private readonly ILoggerService _loggerService;

        public LogsController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string level = null,
            [FromQuery] string search = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            // Используем твой сервис
            var logs = await _loggerService.GetLogsAsync(page, pageSize, level, fromDate, toDate, search);
            var total = await _loggerService.GetTotalCountAsync(level, fromDate, toDate, search);
            var levels = await _loggerService.GetLogLevelsAsync();

            var logEntries = logs.Select(log => new LogEntry
            {
                Id = log.Id,
                Level = log.Level,
                User = log.User,
                Message = log.Message,
                Logger = log.Logger,
                Timestamp = log.Timestamp
            }).ToList();

            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Total"] = total;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            ViewData["Levels"] = levels;
            ViewData["SelectedLevel"] = level;
            ViewData["Search"] = search;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            return View(logEntries);
        }

        [HttpGet]
        [Route("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _loggerService.GetLogByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            return View(log);
        }

        [HttpGet]
        [Route("clear")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ClearOldLogs([FromQuery] int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            await _loggerService.ClearOldLogsAsync(cutoffDate);

            TempData["Message"] = $"Логи старше {cutoffDate:dd.MM.yyyy} удалены";
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> DeleteSelected([FromForm] List<int> logIds)
        {
            if (logIds != null && logIds.Count > 0)
            {
            // Используем сервис!
                await _loggerService.DeleteLogsAsync(logIds);
                            
                TempData["Message"] = $"Удалено {logIds.Count} логов";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Не выбрано ни одного лога для удаления";
                TempData["MessageType"] = "warning";
            }

            return RedirectToAction("Index");
        }
    }
}