using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
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
       // private const string DRONE_BASE_URL = "http://85.141.101.21:8080";

        private const string DRONE_BASE_URL = "http://192.168.1.159:8080";

        /// <summary>
        /// Взлёт на определённую высоту или посадка
        /// </summary>
        [HttpPost("SystemCheck")]
        public async Task<IActionResult> SystemCheck(bool isActive, int distance)
        {
            try
            {
                string operation = isActive ? $"ВЗЛЕТ на {distance} метров" : "ПОСАДКА";
                _logger.LogMessage(User, $"🚀 Команда: {operation}", LogLevel.Information);

                // 1. Записываем событие в БД
                EventRegistration registration = new()
                {
                    EventId = 2,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _flightServece.AddEventRegistration(registration);

                // 2. Формируем команду для дрона
                DroneCommand command = new()
                {
                    Takeoff = isActive,
                    Altitude = distance
                };

                string json = JsonSerializer.Serialize(command);
                _logger.LogMessage(User, $"📤 Отправляемая команда: {json}", LogLevel.Information);

                // 3. Отправляем команду дрону через сервис
                var result = await _droneConnection.SendCommandToDrone("takeoff-land", command);

                // 4. Логируем результат
                if (result.Success)
                {
                    _logger.LogMessage(User, $"✅ {operation} успешно отправлена дрону", LogLevel.Information);
                    
                    // Проверяем ответ от дрона (если нужно)
                    // Здесь мы не парсим response, так как SendCommandToDrone уже проверяет ошибки
                    
                    return Ok(new { 
                        status = "success", 
                        message = $"{operation} команда принята дроном"
                    });
                }
                else
                {
                    _logger.LogMessage(User, $"❌ Ошибка отправки команды: {result.ErrorMessage}", LogLevel.Error);
                    
                    return Ok(new { 
                        status = "error", 
                        message = $"Не удалось отправить команду дрону: {result.ErrorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"💥 Исключение: {ex.Message}", LogLevel.Error);
                return Ok(new { 
                    status = "error", 
                    message = $"Исключение: {ex.Message}"
                });
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

        /// <summary>
        /// логирование на серверСбросс всех заданий
        /// </summary>
        /// <param name="stop"></param>
        /// <returns></returns>
        [HttpPost("reset")]
        public IActionResult Reset(bool stop)
        {
            // log содержит поля из systemd journal
            // Сохрани в базу: log.Message, log.Timestamp, log.Unit и т.д.
            
            return Ok("success");
        }

        /// <summary>
        /// Возвращение на домашнюю позицию
        /// </summary>
        /// <param name="stat"></param>
        /// <returns></returns>
        [HttpPost("home")]
        public IActionResult HomePosition(bool stat)
        {
            // log содержит поля из systemd journal
            // Сохрани в базу: log.Message, log.Timestamp, log.Unit и т.д.
            return Ok("success");
        }
		
		/// <summary>
        /// логирование на сервер
        /// </summary>
        /// <param name="logData"></param>
        /// <returns></returns>
        [HttpPost("logs")]
        public IActionResult Post([FromBody] Dictionary<string, object> logData)
        {
            try
            {
                // logData содержит все поля из journald JSON
                var message = logData.ContainsKey("MESSAGE") ? logData["MESSAGE"].ToString() : "No message";
                var timestamp = logData.ContainsKey("__REALTIME_TIMESTAMP") ? logData["__REALTIME_TIMESTAMP"].ToString() : "";
                var unit = logData.ContainsKey("_SYSTEMD_UNIT") ? logData["_SYSTEMD_UNIT"].ToString() : "";
                
                // Сохрани в базу
                _logger.LogMessage(User, $"Received log: {message}", LogLevel.Warning);
                
                return Ok(new { received = true, message = "Log saved" });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, "Error processing log", LogLevel.Warning);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}