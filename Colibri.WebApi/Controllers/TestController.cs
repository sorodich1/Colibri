using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

namespace Colibri.WebApi.Controllers
{
    /// <summary>
    /// Тестовый контроллер для роли "Техник"
    /// </summary>
    /// <param name="logger">Логирование данных</param>
    /// <param name="flightServece">Полётный сервис</param>
    /// <param name="telemetry">Сервис телеметрии</param>
    /// <param name="http">Сервис телеметрии</param>
    /// <param name="missionPlanning">Сервис телеметрии</param>
    /// <param name="droneConnection">Сервис телеметрии</param>
    [Route("test")]
    [ApiController]
    public class TestController(ILoggerService logger, IFlightService flightServece, ITelemetryServices telemetry,
     IHttpConnectService http, IMissionPlanningService missionPlanning, IDroneConnectionService droneConnection) : ControllerBase
    {
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightServece = flightServece;
        private readonly ITelemetryServices _telemetry = telemetry;
        private readonly IHttpConnectService _http = http;
        private readonly IMissionPlanningService _missionPlanning = missionPlanning;
        private readonly IDroneConnectionService _droneConnection = droneConnection;

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

                var json = JsonConvert.SerializeObject(command);

                // Используем сервис подключения к дрону вместо жесткого URL
                var result = await _droneConnection.SendCommandToDrone("api/flight/command", json);

                if (!result.Success)
                {
                    _logger.LogMessage(User, "Не удалось отправить команду взлёта/посадки", LogLevel.Error);
                    return Ok("error");
                }

                return Ok("success");
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
        /// <param name="colorNumber">Номер цвета -- 0 - цвет выключен, 1 - зелёный, 2 - красный</param>
        /// <returns>success - запрос выполнен, error - запрос не выполнен</returns>
        [Authorize]
        [HttpPost("BacklightTesting")]
        public async Task<IActionResult> BacklightTesting(int colorNumber)
        {
            try
            {
                _logger.LogMessage(User, $"Выбран цвет с номером {colorNumber}", LogLevel.Information);

                EventRegistration registration = new()
                {
                    EventId = 1,
                    IsActive = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _flightServece.AddEventRegistration(registration);

                // Отправляем команду на дрон для управления светодиодами
                var result = await _droneConnection.SendCommandToDrone("api/led/control", colorNumber.ToString());

                if (!result.Success)
                {
                    _logger.LogMessage(User, "Не удалось отправить команду подсветки", LogLevel.Error);
                    return Ok("error");
                }

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
        
        [HttpPost("TestAutopilot")]
        public async Task<IActionResult> TestAutopilot(double latitude, double longitude)
        {
            try
            {
                _logger.LogMessage(User, $"Тестируется полёт по координатам широта - {latitude}, долгота - {longitude}", LogLevel.Information);

                // Получаем текущий активный дрон
                var activeDroneUrl = "http://85.141.101.21:8080"; // await _droneConnection.GetActiveDroneUrl();
                if (string.IsNullOrEmpty(activeDroneUrl))
                {
                    _logger.LogMessage(User, "Нет доступных дронов для подключения", LogLevel.Error);
                    return Ok("error: нет доступных дронов");
                }

                // Получаем текущую позицию дрона
                var dronePosition = await _missionPlanning.GetCurrentDronePosition(activeDroneUrl);
                var startPoint = dronePosition.Position;

                // Используем сервис для создания миссии
                var mission = await _missionPlanning.CreateDeliveryMission(
                    startPoint, 
                    new GeoPoint { Latitude = latitude, Longitude = longitude, Altitude = 10 }, 
                    cruiseSpeed: 15, 
                    altitude: 10);

                // Логируем миссию
                _logger.LogMessage(User, $"Отправляемая миссия: {JsonConvert.SerializeObject(mission)}", LogLevel.Error);

                // Отправляем миссию напрямую на execute-mission
                var result = await _droneConnection.SendCommandToDrone("execute-mission", mission);

                if (!result.Success)
                {
                    _logger.LogMessage(User, "Не удалось отправить миссию на дрон", LogLevel.Error);
                    return Ok("error: не удалось отправить миссию на дрон");
                }

                // Логируем успех
                await LogMissionCreation(startPoint, new GeoPoint { Latitude = latitude, Longitude = longitude, Altitude = 10 }, "mission_executed");

                _logger.LogMessage(User, $"Миссия успешно отправлена на дрон: {result.DroneUrl}", LogLevel.Information);

                return Ok("success");
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return Ok("error: " + ex.Message);
            }
        }

        private async Task LogMissionCreation(GeoPoint start, GeoPoint end, string missionId)
        {
            try
            {
                var registration = new EventRegistration
                {
                    EventId = 3, // Test Mission Created
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false,
                    AdditionalData = $"Test mission {missionId} from {start.Latitude},{start.Longitude} to {end.Latitude},{end.Longitude}"
                };

                await _flightServece.AddEventRegistration(registration);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка логирования тестовой миссии: {ex.Message}", LogLevel.Warning);
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
            catch (Exception ex)
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


        [HttpGet("DroneStatus")]
        public async Task<IActionResult> GetDroneStatus()
        {
            try
            {
                var activeDroneUrl = await _droneConnection.GetActiveDroneUrl();
                if (string.IsNullOrEmpty(activeDroneUrl))
                {
                    return Ok(new { status = "no_drones_available" });
                }

                // Проверяем статус через endpoint /status
                var result = await _droneConnection.SendCommandToDrone("status", new { });

                if (result.Success)
                {
                    return Ok(new
                    {
                        status = "connected",
                        droneUrl = activeDroneUrl,
                        message = "Дрон доступен и готов к работе"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = "disconnected",
                        droneUrl = activeDroneUrl,
                        message = "Дрон недоступен"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("CheckDroneEndpoints")]
        public async Task<IActionResult> CheckDroneUrls()
        {
            // Временная заглушка - посмотрим какие URL используются
            var droneUrls = new[]
            {
                "http://78.25.108.95:8080",
                "http://78.25.108.95:8081",
                "http://85.141.101.21:8080",
                "http://85.141.101.21:8081"
            };

            var results = new List<object>();

            foreach (var url in droneUrls)
            {
                try
                {
                    // Пробуем отправить тестовый запрос
                    var result = await _droneConnection.SendCommandToDrone("api/health", new { });

                    results.Add(new
                    {
                        url = url,
                        status = result.Success ? "available" : "unavailable",
                        active = result.DroneUrl == url
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        url = url,
                        status = "error",
                        error = ex.Message
                    });
                }
            }

            return Ok(results);
        }
        [HttpGet]
        public async Task<IActionResult> GetDronePosition()
        {
            try
            {
                // Получаем URL дрона
                var droneUrl = "http://85.141.101.21:8080"; // await _droneConnection.GetActiveDroneUrl();
                if (string.IsNullOrEmpty(droneUrl))
                {
                    return Ok(new
                    {
                        error = "Дрон недоступен",
                        latitude = 0,
                        longitude = 0,
                        altitude = 0
                    });
                }

                // Получаем позицию дрона
                var dronePosition = await _missionPlanning.GetCurrentDronePosition(droneUrl);

                return Ok(new
                {
                    latitude = dronePosition.Position.Latitude,
                    longitude = dronePosition.Position.Longitude,
                    altitude = dronePosition.Position.Altitude,
                    status = dronePosition.Status
                });
            }
            catch
            {
                // Возвращаем заглушку при ошибке
                return Ok(new
                {
                    latitude = 0,
                    longitude = 0,
                    altitude = 0,
                    battery = 0,
                    status = "error"
                });
            }
        }
        
    }
}
