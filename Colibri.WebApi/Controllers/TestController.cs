using System;
using System.Text.Json;
using System.Threading.Tasks;
using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Тестовый контроллер для роли "Техник"
    /// </summary>
    /// <param name="logger">Логирование данных</param>
    /// <param name="flightServece">Полётный сервис</param>
    /// <param name="telemetry">Сервис телеметрии</param>
    [Route("test")]
    [ApiController]
    public class TestController(ILoggerService logger, IFlightService flightServece, ITelemetryServices telemetry, IHttpConnectService http) : ControllerBase
    {
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightServece = flightServece;
        private readonly ITelemetryServices _telemetry = telemetry;
        private readonly IHttpConnectService _http = http;
        private readonly string _droneBaseUrl = "http://78.25.108.95:8080";

        /// <summary>
        /// Тестовый метод взлёта на определённую высоту
        /// </summary>
        /// <param name="isActive">Работа дрона true - взлёт, false - посадка</param>
        /// <param name="distance">Высота взлёта в метрах</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("SystemCheck")]
        public async Task<IActionResult> SystemCheck(bool isActive, int distance)
        {
            try
            {
                EventRegistration registration = new()
                {
                    EventId = 2,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _flightServece.AddEventRegistration(registration);

                FlightCommand command = new()
                {
                    AltitudeMeters = distance,
                    ShouldTakeoff = isActive,
                    CommandId = Guid.NewGuid().ToString()
                };

                var json = JsonSerializer.Serialize(command);

                var response = await _http.PostAsync(_droneBaseUrl, json);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error");
            }
        }

        /// <summary>
        /// Тестирование подсветки дрона
        /// </summary>
        /// <param name="colorNumber">Номер цвета -- 0 - цвет выключен, 1 - зелёный, 2 - крассный</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("BacklightTesting")]
        public async Task<IActionResult> BacklightTesting(int colorNumber)
        {
            try
            {
                _logger.LogMessage(User, $"Выбран цвет с номером {colorNumber}", LogLevel.Information);

                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error");
            }
        }

        /// <summary>
        /// Тестирование автопилота по заданным координатам
        /// </summary>
        /// <param name="latitude">Широта</param>
        /// <param name="longitude">Долгота</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("TestAutopilot")]
        public async Task<IActionResult> TestAutopilot(double latitude, double longitude)
        {
            try
            {
                _logger.LogMessage(User, $"Тестируется полёт по координатам широта - {latitude}, долгота - {longitude}", LogLevel.Information);

                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error");
            }
        }

        /// <summary>
        /// Обрабатывает получение телеметрических данных от БПЛА
        /// </summary>
        /// <param name="telemetryData">Объект с телеметрическими данными БПЛА</param>
        /// <returns>Результат обработки телеметрии</returns>
        [HttpPost("PostTelemetryData")]
        [ProducesResponseType(typeof(TelemetryResponse), 200)] // Успешная обработка
        [ProducesResponseType(typeof(TelemetryResponse), 400)] // Ошибка валидации
        public async Task<IActionResult> PostTelemetryData([FromBody] TelemetryData telemetryData)
        {
            try
            {
                 // Проверка на null входных данных
                if (telemetryData == null)
                {
                    return BadRequest(new TelemetryResponse
                    {
                        Message = "Telemetry data is required",
                        Success = false
                    });
                }

                // Асинхронная обработка телеметрии сервисом
                var result = await _telemetry.ProcessTelemetryAsync(telemetryData);

                // Возврат результата в зависимости от успешности обработки
                if (result.Success)
                {
                    return Ok(result); // 200 OK
                }
                else
                {
                    return BadRequest(result); // 400 Bad Request
                }
            }
            catch(Exception ex)
            {
                // Логирование детальной информации об ошибке
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);

                // Возврат ошибки сервера без деталей клиенту (безопасность)
                return StatusCode(500, new TelemetryResponse
                {
                    Message = ex.Message,
                    Success = false
                });

            }
        } 
    }
}
