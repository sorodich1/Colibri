using System;
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
    [Route("test")]
    [ApiController]
    public class TestController(ILoggerService logger, IFlightService flightServece, 
                               IMissionPlanningService missionPlanning, IDroneConnectionService droneConnection) : ControllerBase
    {
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightServece = flightServece;
        private readonly IMissionPlanningService _missionPlanning = missionPlanning;
        private readonly IDroneConnectionService _droneConnection = droneConnection;

        // Базовый URL дрона
        private const string DRONE_BASE_URL = "http://192.168.1.159:8080";

        /// <summary>
        /// Взлёт на определённую высоту
        /// </summary>
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

                var result = await _droneConnection.SendCommandToDrone($"{DRONE_BASE_URL}/api/flight/command", json);

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
        /// Управление цветом подсветки
        /// </summary>
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

                var result = await _droneConnection.SendCommandToDrone($"{DRONE_BASE_URL}/api/led/control", colorNumber.ToString());

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
        /// Полёт по гео точкам
        /// </summary>
        [HttpPost("TestAutopilot")]
        public async Task<IActionResult> TestAutopilot(double latitude, double longitude)
        {
            try
            {
                _logger.LogMessage(User, $"Тестируется полёт по координатам широта - {latitude}, долгота - {longitude}", LogLevel.Information);

                var activeDroneUrl = DRONE_BASE_URL;

                // Получаем текущую позицию дрона
                var dronePosition = await _missionPlanning.GetCurrentDronePosition(activeDroneUrl);
                var startPoint = dronePosition.Position;

                // Создаем миссию
                var mission = await _missionPlanning.CreateDeliveryMission(
                    startPoint,
                    new GeoPoint { Latitude = latitude, Longitude = longitude, Altitude = 5 },
                    cruiseSpeed: 15,
                    altitude: 5);

                // Отправляем миссию
                var result = await _droneConnection.SendCommandToDrone($"execute-mission", mission);

                if (!result.Success)
                {
                    _logger.LogMessage(User, "Не удалось отправить миссию на дрон", LogLevel.Error);
                    return Ok("error: не удалось отправить миссию на дрон");
                }

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

        /// <summary>
        /// Получить текущие координаты дрона
        /// </summary>
        [HttpGet("GetDronePosition")]
        public async Task<IActionResult> GetDronePosition()
        {
            try
            {
                var activeDroneUrl = DRONE_BASE_URL;

                var position = await _missionPlanning.GetCurrentDronePosition(activeDroneUrl);
                
                return Ok(new {
                    latitude = position.Position.Latitude,
                    longitude = position.Position.Longitude, 
                    altitude = position.Position.Altitude,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка получения координат: {ex.Message}", LogLevel.Error);
                return Ok(new { error = ex.Message });
            }
        }

        private async Task LogMissionCreation(GeoPoint start, GeoPoint end, string missionId)
        {
            try
            {
                var registration = new EventRegistration
                {
                    EventId = 3,
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
    }
}