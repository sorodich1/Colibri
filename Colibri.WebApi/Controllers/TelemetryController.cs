using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.DTO;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Controllers
{
     [Route("telemetry")]
    public class TelemetryController(ILoggerService logger, ITelemetryService telemetryService) : Controller
    {
        private readonly ITelemetryService _telemetryService = telemetryService;
        private readonly ILoggerService _logger = logger;

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 50,
            [FromQuery] string search = null, 
            [FromQuery] DateTime? fromDate = null, 
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? gpsStatus = null)
        {
            var telemetries = await _telemetryService.GetTelemetriesAsync(
                page, pageSize, fromDate, toDate, search, gpsStatus);
            
            var total = await _telemetryService.GetTotalCountAsync(
                fromDate, toDate, search, gpsStatus);
            
            var gpsStatuses = await _telemetryService.GetGpsStatusesAsync();

            var telemetryEntries = telemetries.Select(t => new TelemetryEntry
            {
                Id = t.Id,
                Latitude = t.Latitude,
                Longitude = t.Longitude,
                Altitude = t.Altitude,
                RelativeAltitude = t.RelativeAltitude,
                BatteryVoltage = t.BatteryVoltage,
                BatteryPercentage = t.BatteryPercentage,
                Gyro = t.Gyro,
                Accel = t.Accel,
                Mag = t.Mag,
                GpsStatus = t.GpsStatus,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            }).ToList();

            ViewData["Page"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["Total"] = total;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            ViewData["GpsStatuses"] = gpsStatuses;
            ViewData["SelectedGpsStatus"] = gpsStatus?.ToString();
            ViewData["Search"] = search;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            return View(telemetryEntries);
        }

        [HttpGet]
        [Route("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var telemetry = await _telemetryService.GetTelemetryByIdAsync(id);
            if (telemetry == null)
            {
                return NotFound();
            }
            
            var telemetryEntry = new TelemetryEntry
            {
                Id = telemetry.Id,
                Latitude = telemetry.Latitude,
                Longitude = telemetry.Longitude,
                Altitude = telemetry.Altitude,
                RelativeAltitude = telemetry.RelativeAltitude,
                BatteryVoltage = telemetry.BatteryVoltage,
                BatteryPercentage = telemetry.BatteryPercentage,
                Gyro = telemetry.Gyro,
                Accel = telemetry.Accel,
                Mag = telemetry.Mag,
                GpsStatus = telemetry.GpsStatus,
                CreatedAt = telemetry.CreatedAt,
                UpdatedAt = telemetry.UpdatedAt
            };
            
            return View(telemetryEntry);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> DeleteSelected([FromForm] List<int> telemetryIds)
        {
            if (telemetryIds != null && telemetryIds.Count > 0)
            {
                await _telemetryService.DeleteTelemetriesAsync(telemetryIds);
                
                TempData["Message"] = $"Удалено {telemetryIds.Count} записей телеметрии";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "Не выбрано ни одной записи для удаления";
                TempData["MessageType"] = "warning";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("clear-old")]
        public async Task<IActionResult> ClearOld([FromForm] int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            await _telemetryService.ClearOldTelemetriesAsync(cutoffDate);

            TempData["Message"] = $"Записи телеметрии старше {cutoffDate:dd.MM.yyyy} удалены";
            TempData["MessageType"] = "success";
            
            return RedirectToAction("Index");
        }

                /// <summary>
        /// Прием телеметрии от дрона
        /// </summary>
        [HttpPost]
        [Route("api/telemetry")]
        public async Task<IActionResult> ReceiveTelemetry([FromBody] DroneTelemetryDto telemetryDto)
        {
            try
            {
                if (telemetryDto == null)
                {
                    return BadRequest(new { error = "Пустой запрос" });
                }

                // Валидация основных полей
                if (telemetryDto.Latitude < -90 || telemetryDto.Latitude > 90)
                {
                    _logger.LogMessage(User, $"Некорректная широта: {telemetryDto.Latitude}", LogLevel.Error);
                    return BadRequest(new { error = "Некорректная широта" });
                }

                if (telemetryDto.Longitude < -180 || telemetryDto.Longitude > 180)
                {
                    _logger.LogMessage(User, $"Некорректная долгота: {telemetryDto.Longitude}", LogLevel.Error);
                    return BadRequest(new { error = "Некорректная долгота" });
                }

                // Создаем сущность для сохранения в БД
                var telemetry = new Telemetry
                {
                    Latitude = telemetryDto.Latitude,
                    Longitude = telemetryDto.Longitude,
                    Altitude = telemetryDto.Altitude,
                    RelativeAltitude = telemetryDto.RelativeAltitude,
                    BatteryVoltage = telemetryDto.BatteryVoltage,
                    BatteryPercentage = telemetryDto.BatteryPercentage,
                    Gyro = telemetryDto.Gyro,
                    Accel = telemetryDto.Accel,
                    Mag = telemetryDto.Mag,
                    GpsStatus = telemetryDto.GpsStatus ?? "NO_FIX",
                    Satellites = telemetryDto.Satellites,
                    CreatedAt = telemetryDto.Timestamp,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _telemetryService.AddTelemetryAsync(telemetry);

                // Возвращаем успешный ответ
                return Ok(new 
                { 
                    status = "success",
                    message = "Телеметрия получена и сохранена",
                    telemetry_id = telemetry.Id,
                    timestamp = telemetry.CreatedAt
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(jsonEx), LogLevel.Error);
                return BadRequest(new { error = "Некорректный JSON формат", details = jsonEx.Message });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(dbEx), LogLevel.Error);
                return StatusCode(500, new { error = "Ошибка сохранения данных", details = dbEx.InnerException?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера", details = ex.Message });
            }
        }
    }
}
