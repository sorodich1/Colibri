using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.GetDirection;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    [Route("flight")]
    [ApiController]
    public class FlightController : Controller
    {
        private readonly ILoggerService _logger;
        private readonly IFlightService _flightService;
        private readonly IDroneConnectionService _droneConnection;
        private readonly IMissionPlanningService _missionPlanning;
        private readonly IConfiguration _configuration;

        public FlightController(
            IConfiguration configuration,
            ILoggerService logger,
            IFlightService flightService,
            IDroneConnectionService droneConnection,
            IMissionPlanningService missionPlanning
            )
        {
            _logger = logger;
            _flightService = flightService;
            _droneConnection = droneConnection;
            _configuration = configuration;
            _missionPlanning = missionPlanning;
        }

        /// <summary>
        /// Передача гео точек
        /// </summary>
        [HttpPost("orderlocation")]
        public async Task<IActionResult> GeodataTransfer([FromBody] OrderLocation routeResponse)
        {
            try
            {
                // Получаем маршрут
                var json = await DirectionJson.RouteFly(routeResponse.SellerPoint, routeResponse.BuyerPoint);
                var jsonMission = await DirectionJson.MissionFile(json);

                // Отправляем маршрут на дрон
                var command = new
                {
                    Command = "SET_MISSION",
                    Mission = jsonMission,
                    Timestamp = DateTime.UtcNow
                };

                var result = await _droneConnection.SendCommandToDrone("api/flight/mission", command);

                if (!result.Success)
                {
                    return StatusCode(503, new { error = "Дрон недоступен", details = result.ErrorMessage });
                }

                return Ok(new { 
                    mission = jsonMission,
                    drone = result.DroneUrl,
                    responseTime = result.ResponseTime.TotalMilliseconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new { error = "Ошибка планирования маршрута" });
            }
        }

        /// <summary>
        /// Открытие/закрытие бокса дрона
        /// </summary>
        [Authorize]
        [HttpPost("openbox")]
        public async Task<IActionResult> OpenBox([FromBody] bool isActive)
        {
            try
            {
                _logger.LogMessage(User, $"Открытие бокса дрона - {isActive}", LogLevel.Information);
                
                // Логируем событие в БД
                var registration = new EventRegistration
                {
                    EventId = 1,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _flightService.AddEventRegistration(registration);

                // Отправляем команду на дрон
                var command = new
                {
                    Command = isActive ? "OPEN_BOX" : "CLOSE_BOX",
                    Timestamp = DateTime.UtcNow
                };

                var result = await _droneConnection.SendCommandToDrone("api/actuator/control", command);

                if (!result.Success)
                {
                    return StatusCode(503, new { error = "Не удалось отправить команду дрону" });
                }

                return Ok(new { 
                    status = "success",
                    command = isActive ? "open" : "close",
                    drone = result.DroneUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Запуск полета
        /// </summary>
        [Authorize]
        [HttpPost("start")]
        public async Task<IActionResult> StartFlight()
        {
            var command = new { Command = "START_FLIGHT", Timestamp = DateTime.UtcNow };
            var result = await _droneConnection.SendCommandToDrone("api/flight/control", command);
            
            if (!result.Success)
                return StatusCode(503, new { error = "Дрон недоступен" });

            return Ok(new { status = "flight_started", drone = result.DroneUrl });
        }

        /// <summary>
        /// Экстренная остановка
        /// </summary>
        [Authorize]
        [HttpPost("emergency")]
        public async Task<IActionResult> EmergencyStop()
        {
            var command = new { Command = "EMERGENCY_STOP", Timestamp = DateTime.UtcNow };
            var result = await _droneConnection.SendCommandToDrone("api/flight/emergency", command);
            
            if (!result.Success)
                return StatusCode(503, new { error = "Дрон недоступен" });

            return Ok(new { status = "emergency_stop", drone = result.DroneUrl });
        }

        /// <summary>
        /// Получение статуса дрона
        /// </summary>
        [Authorize]
        [HttpGet("status")]
        public async Task<IActionResult> GetDroneStatus()
        {
            var result = await _droneConnection.SendCommandToDrone("api/flight/status", new { Command = "GET_STATUS" });
            
            if (!result.Success)
                return StatusCode(503, new { error = "Дрон недоступен" });

            return Ok(new { status = "connected", drone = result.DroneUrl });
        }
    }
}