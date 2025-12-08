using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Colibri.Data.Entity;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryController(ILoggerService logger, ITelemetryService telemetry) : ControllerBase
    {
        private readonly ILoggerService _logger = logger;
        private readonly ITelemetryService _telemetry = telemetry;

         /// <summary>
        /// Получение и сохранение телеметрии от дрона
        /// </summary>
        /// <param name="telemetryData">Данные телеметрии в формате JSON</param>
        [HttpPost]
        public async Task<IActionResult> ReceiveTelemetry([FromBody] DroneTelemetryDto telemetryData)
        {
            try
            {
                _logger.LogMessage(User, $"📡 Получена телеметрия от дрона", LogLevel.Information);
                
                // Логируем полученные данные
                _logger.LogMessage(User, 
                    $"📍 Позиция: {telemetryData.Latitude:F6}, {telemetryData.Longitude:F6}, " +
                    $"Высота: {telemetryData.Altitude:F2}м, Относ. высота: {telemetryData.RelativeAltitude:F2}м", 
                    LogLevel.Information);
                
                _logger.LogMessage(User, 
                    $"🔋 Батарея: {telemetryData.BatteryPercentage:F1}% ({telemetryData.BatteryVoltage:F2}V), " +
                    $"Спутники: {telemetryData.Satellites}, Статус GPS: {telemetryData.GpsStatus}", 
                    LogLevel.Information);

                // Создаем сущность для сохранения в БД
                var telemetryEntity = new Telemetry
                {
                    Latitude = telemetryData.Latitude,
                    Longitude = telemetryData.Longitude,
                    Altitude = telemetryData.Altitude,
                    RelativeAltitude = telemetryData.RelativeAltitude,
                    BatteryVoltage = telemetryData.BatteryVoltage,
                    BatteryPercentage = telemetryData.BatteryPercentage,
                    Gyro = telemetryData.Gyro,
                    Accel = telemetryData.Accel,
                    Mag = telemetryData.Mag,
                    GpsStatus = telemetryData.GpsStatus,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                // Сохраняем в базу данных
                await _telemetry.SaveTelemetryAsync(telemetryEntity);

                _logger.LogMessage(User, $"✅ Телеметрия сохранена в БД с ID: {telemetryEntity.Id}", LogLevel.Information);

                return Ok(new 
                { 
                    Status = "success",
                    Message = "Telemetry received and saved",
                    TelemetryId = telemetryEntity.Id,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogMessage(User, $"❌ Ошибка парсинга JSON телеметрии: {jsonEx.Message}", LogLevel.Error);
                return BadRequest(new 
                { 
                    Status = "error", 
                    Message = "Invalid JSON format",
                    Details = jsonEx.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"💥 Ошибка обработки телеметрии: {ex.Message}", LogLevel.Error);
                return StatusCode(500, new 
                { 
                    Status = "error", 
                    Message = "Internal server error",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Получение последней телеметрии дрона
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestTelemetry()
        {
            try
            {
                var latestTelemetry = await _telemetry.GetLatestTelemetryAsync();
                
                if (latestTelemetry == null)
                {
                    return NotFound(new { Status = "not_found", Message = "No telemetry data available" });
                }

                var result = new DroneTelemetryDto
                {
                    Latitude = latestTelemetry.Latitude,
                    Longitude = latestTelemetry.Longitude,
                    Altitude = latestTelemetry.Altitude,
                    RelativeAltitude = latestTelemetry.RelativeAltitude,
                    BatteryVoltage = latestTelemetry.BatteryVoltage,
                    BatteryPercentage = latestTelemetry.BatteryPercentage,
                    Gyro = latestTelemetry.Gyro,
                    Accel = latestTelemetry.Accel,
                    Mag = latestTelemetry.Mag,
                    GpsStatus = latestTelemetry.GpsStatus,
                    Timestamp = latestTelemetry.CreatedAt,
                    Satellites = 0 // Можно добавить в сущность если нужно
                };

                return Ok(new
                {
                    Status = "success",
                    Data = result,
                    ReceivedAt = latestTelemetry.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка получения телеметрии: {ex.Message}", LogLevel.Error);
                return StatusCode(500, new { Status = "error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Получение телеметрии за период
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetTelemetryHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                from ??= DateTime.Now.AddHours(-1); // Последний час по умолчанию
                to ??= DateTime.Now;

                var telemetryList = await _telemetry.GetTelemetryByPeriodAsync(from.Value, to.Value);
                
                var result = telemetryList.Select(t => new DroneTelemetryDto
                {
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
                    Timestamp = t.CreatedAt
                }).ToList();

                return Ok(new
                {
                    Status = "success",
                    Count = result.Count,
                    Period = $"{from:yyyy-MM-dd HH:mm:ss} - {to:yyyy-MM-dd HH:mm:ss}",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка получения истории телеметрии: {ex.Message}", LogLevel.Error);
                return StatusCode(500, new { Status = "error", Message = ex.Message });
            }
        }

        /// <summary>
        /// Статистика телеметрии
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetTelemetryStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                from ??= DateTime.Now.AddHours(-1);
                to ??= DateTime.Now;

                var stats = await _telemetry.GetTelemetryStatsAsync(from.Value, to.Value);
                
                return Ok(new
                {
                    Status = "success",
                    Period = $"{from:yyyy-MM-dd HH:mm:ss} - {to:yyyy-MM-dd HH:mm:ss}",
                    Stats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка получения статистики телеметрии: {ex.Message}", LogLevel.Error);
                return StatusCode(500, new { Status = "error", Message = ex.Message });
            }
        }
    }
}
