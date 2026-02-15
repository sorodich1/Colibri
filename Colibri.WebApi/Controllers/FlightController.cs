using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.GetDirection;
using Colibri.WebApi.Enum;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Colibri.WebApi.Controllers
{
    [Route("flight")]
    [ApiController]
    public class FlightController(
        IClientOrderService clientOrder,
        ILoggerService logger,
        IFlightService flightService,
        IDroneConnectionService droneConnection,
        IMissionPlanningService missionPlanning,
        IHomePositionService homePositionService,
        IOrderStatusService orderStatusService
            ) : Controller
    {
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightService = flightService;
        private readonly IDroneConnectionService _droneConnection = droneConnection;
        private readonly IMissionPlanningService _missionPlanning = missionPlanning;
        private readonly IHomePositionService _homePositionService = homePositionService;
        private readonly IClientOrderService _clientOrder = clientOrder;

        private readonly IOrderStatusService _orderStatusService = orderStatusService;

        /// <summary>
        /// Передача гео точек0
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

                return Ok(new
                {
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

                return Ok(new
                {
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

        /// <summary>
        /// Отправить дрон продавцу (кнопка - "Подтвердить геолокацию")
        /// </summary>
        [Authorize]
        [HttpPost("сonfirmпeolocation")]
        public async Task<IActionResult> SetConfirmGeolocation([FromBody]ConfirmGeolocationRequest model)
        {
            try
            {
                if (model.GeoPoint == null)
                {
                    _logger.LogMessage(User, "Пустой запрос или отсутствуют точки маршрута", LogLevel.Warning);
                    return BadRequest(new { error = "Отсутствуют точки маршрута" });
                }

                _logger.LogMessage(User, $"Отправка дрона с дронбокса продавцу - получено {model.GeoPoint.Longitude} {model.GeoPoint.Latitude}", LogLevel.Information);


                // 1. Получаем текущую позицию дрона
                var droneUrl = await _droneConnection.GetActiveDroneUrl();

                var dronePosition = await _missionPlanning.GetCurrentDronePosition(droneUrl);

                if (dronePosition == null)
                {
                    _logger.LogMessage(User, "Не удалось получить текущую позицию дрона", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось получить позицию дрона" });
                }

                var startPoint = dronePosition.Position;
                _logger.LogMessage(User, $"Текущая позиция дрона: Lat={startPoint.Latitude:F6}, Lon={startPoint.Longitude:F6}, Alt={startPoint.Altitude:F1}", LogLevel.Information);

                // 2. Создаем список точек маршрута (от текущей позиции дрона к точке продавца)
                var waypoints = new List<GeoPoint> { model.GeoPoint };

                // 3. Создаем миссию
                var mission = await _missionPlanning.CreateFullQgcMission(
                    startPoint: startPoint,
                    waypoints: waypoints,
                    returnToHome: false // Посадка в точке продавца
                );

                if (mission == null)
                {
                    _logger.LogMessage(User, "Не удалось создать миссию", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось создать миссию" });
                }

                // 4. Отправляем миссию на дрон
                var result = await _droneConnection.SendCommandToDrone("execute-mission", mission);

                if (!result.Success)
                {
                    _logger.LogMessage(User, $"Не удалось отправить миссию на дрон: {result.ErrorMessage}", LogLevel.Error);
                    return Ok(new
                    {
                        status = "error",
                        message = "Не удалось отправить миссию на дрон",
                        details = result.ErrorMessage
                    });
                }

                // 5. Логируем создание миссии
                //await LogMissionCreation("seller", startPoint, geoPoint);
                _logger.LogMessage(User, $"Миссия успешно отправлена на дрон! Направление: к продавцу", LogLevel.Information);



                var order = await _clientOrder.GetOrderByIdAsync(model.OrderId);

                order.Status = "IS_GOING_TO";

                await _clientOrder.UpdateOrdersAsync(order);

                await _orderStatusService.NotifyOrderUpdateAsync(model.OrderId, order.Status, new
                    {
                        changed_by = User.Identity.Name,
                        timestamp = DateTime.UtcNow
                    });


                return Ok(new
                {
                    status = "success",
                    message = "Дрон отправлен к продавцу",
                    start_point = new
                    {
                        latitude = startPoint.Latitude,
                        longitude = startPoint.Longitude,
                        altitude = startPoint.Altitude
                    },
                    destination = new
                    {
                        latitude = model.GeoPoint.Latitude,
                        longitude = model.GeoPoint.Longitude
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message,
                    details = ex.StackTrace
                });
            }             
        }

        /// <summary>
        /// Отправить дрон покупателю (кнопка - "Отправить дрон")
        /// </summary>
        [Authorize]
        [HttpPost("sendadrone")]
        public async Task<IActionResult> SetSendADrone([FromBody] int orderId)
        {
            try
            {
                _logger.LogMessage(User, $"Отправка дрона покупателю - заказ ID: {orderId}", LogLevel.Information);

                var order = await _clientOrder.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    _logger.LogMessage(User, $"Заказ с ID {orderId} не найден", LogLevel.Warning);
                    return BadRequest(new { error = $"Заказ с ID {orderId} не найден" });
                }

                var buyerGeoPoint = new GeoPoint
                {
                    Latitude = order.DeliveryLatitude,
                    Longitude = order.DeliveryLongitude,
                    Altitude = 2.0
                };

                var droneUrl = await _droneConnection.GetActiveDroneUrl();
                var dronePosition = await _missionPlanning.GetCurrentDronePosition(droneUrl);

                if (dronePosition == null)
                {
                    _logger.LogMessage(User, "Не удалось получить позицию дрона", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось получить позицию дрона" });
                }

                var startPoint = dronePosition.Position;
                var waypoints = new List<GeoPoint> { buyerGeoPoint };

                var mission = await _missionPlanning.CreateFullQgcMission(
                    startPoint: startPoint,
                    waypoints: waypoints,
                    returnToHome: false
                );

                if (mission == null)
                {
                    _logger.LogMessage(User, "Не удалось создать миссию", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось создать миссию" });
                }

                var result = await _droneConnection.SendCommandToDrone("execute-mission", mission);

                if (!result.Success)
                {
                    _logger.LogMessage(User, $"Не удалось отправить миссию: {result.ErrorMessage}", LogLevel.Error);
                    return Ok(new
                    {
                        status = "error",
                        message = "Не удалось отправить миссию",
                        details = result.ErrorMessage
                    });
                }

                order.Status = "IN_TRANSIT";

                await _clientOrder.UpdateOrdersAsync(order);

                await _orderStatusService.NotifyOrderUpdateAsync(orderId, order.Status, new
                    {
                        changed_by = User.Identity.Name,
                        timestamp = DateTime.UtcNow
                    });
                

                return Ok(new
                {
                    status = "success",
                    message = "Дрон отправлен к покупателю",
                    order_id = orderId,
                    start_point = new { startPoint.Latitude, startPoint.Longitude },
                    destination = new { buyerGeoPoint.Latitude, buyerGeoPoint.Longitude },
                    drone = result.DroneUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new { error = "Ошибка отправки дрона покупателю" });
            }
        }

        /// <summary>
        /// Отправить дрон в дронбокс (кнопка - "Подтвердить получение")
        /// </summary>
        [Authorize]
        [HttpPost("confirmreceipt")]
        public async Task<IActionResult> SetConfirmReceipt([FromBody] int orderId)
        {
            try
            {
                _logger.LogMessage(User, "Отправка дрона в дронбокс после получения товара", LogLevel.Information);

                // 1. Получаем текущую позицию дрона
                var droneUrl = await _droneConnection.GetActiveDroneUrl();

                var dronePosition = await _missionPlanning.GetCurrentDronePosition(droneUrl);

                if (dronePosition == null)
                {
                    _logger.LogMessage(User, "Не удалось получить текущую позицию дрона", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось получить позицию дрона" });
                }

                var startPoint = dronePosition.Position;

                // 2. Получаем позицию дронбокса (домашнюю позицию) 

                var homePosition = await _homePositionService.GetHomePosition();

                if (homePosition == null)
                {
                    _logger.LogMessage(User, "ОШИБКА: Домашняя позиция (дронбокс) не установлена", LogLevel.Error);
                    return Ok(new
                    {
                        status = "error",
                        message = "Домашняя позиция дронбокса не установлена",
                        solution = "Сначала запустите миссию через /TestAutopilot"
                    });
                }

                _logger.LogMessage(User, $"Домашняя позиция (дронбокс): Lat={homePosition.Latitude:F6}, Lon={homePosition.Longitude:F6}", LogLevel.Information);

                // 3. Создаем миссию возврата в дронбокс
                var waypoints = new List<GeoPoint> { homePosition };

                var mission = await _missionPlanning.CreateFullQgcMission(
                    startPoint: startPoint,
                    waypoints: waypoints,
                    returnToHome: false // Посадка в дронбоксе
                );

                if (mission == null)
                {
                    _logger.LogMessage(User, "Не удалось создать миссию возврата в дронбокс", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось создать миссию" });
                }

                // 4. Отправляем миссию на дрон
                _logger.LogMessage(User, "Отправляем миссию возврата в дронбокс...", LogLevel.Information);

                var result = await _droneConnection.SendCommandToDrone("execute-mission", mission);

                if (!result.Success)
                {
                    _logger.LogMessage(User, $"Не удалось отправить миссию на дрон: {result.ErrorMessage}", LogLevel.Error);
                    return Ok(new
                    {
                        status = "error",
                        message = "Не удалось отправить миссию на дрон",
                        details = result.ErrorMessage
                    });
                }

                // 5. Логируем создание миссии
               // await LogMissionCreation("dronebox", startPoint, homePosition);

                _logger.LogMessage(User, "УСПЕХ: Команда возврата в дронбокс отправлена на дрон", LogLevel.Information);

                var order = await _clientOrder.GetOrderByIdAsync(orderId);

                order.Status = "DELIVERED";

                await _clientOrder.UpdateOrdersAsync(order);

                await _orderStatusService.NotifyOrderUpdateAsync(orderId, order.Status, new
                    {
                        changed_by = User.Identity.Name,
                        timestamp = DateTime.UtcNow
                    });
                
                return Ok(new
                {
                    status = "success",
                    message = "Дрон возвращается в дронбокс",
                    current_position = new
                    {
                        startPoint.Latitude,
                        startPoint.Longitude
                    },
                    dronebox_position = new
                    {
                        homePosition.Latitude,
                        homePosition.Longitude
                    }
                });
                    
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }
    }
}