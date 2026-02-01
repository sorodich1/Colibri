using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Colibri.Data.Entity;
using Colibri.Data.Helpers;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Colibri.WebApi.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController(ILoggerService logger, IFlightService flightServece, 
                               IMissionPlanningService missionPlanning, IDroneConnectionService droneConnection,
                               IHomePositionService homePositionService) : ControllerBase
    {
        private readonly ILoggerService _logger = logger;
        private readonly IFlightService _flightServece = flightServece;
        private readonly IMissionPlanningService _missionPlanning = missionPlanning;
        private readonly IDroneConnectionService _droneConnection = droneConnection;
        private readonly IHomePositionService _homePositionService = homePositionService;

        // Базовый URL дрона
       // private const string DRONE_BASE_URL = "http://85.141.101.21:8080";
        private const string DRONE_BASE_URL = "http://78.25.108.95:8080";

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

                string json = System.Text.Json.JsonSerializer.Serialize(command);
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
        /// Выбран цвет с определённым номером
        /// </summary>
        /// <param name="colorNumber"></param>
        /// <returns></returns>
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

                // Используем HttpClient напрямую для LED
                using var httpClient = new HttpClient();
                
                // LED контроллер ожидает plain text число
                var content = new StringContent(colorNumber.ToString(), Encoding.UTF8, "text/plain");
                
                var response = await httpClient.PostAsync($"{DRONE_BASE_URL}/api/led/control", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogMessage(User, $"Не удалось отправить команду подсветки. Статус: {response.StatusCode}", LogLevel.Error);
                    return Ok("error");
                }

                var responseText = await response.Content.ReadAsStringAsync();
                _logger.LogMessage(User, $"Ответ от дрона: {responseText}", LogLevel.Information);

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
        public async Task<IActionResult> TestAutopilot([FromBody] GeoMissionRequest request)
        {
            try
                {
                    if (request?.Waypoints == null || request.Waypoints.Count == 0)
                    {
                        _logger.LogMessage(User, "Пустой запрос или отсутствуют точки маршрута", LogLevel.Warning);
                        return BadRequest(new { error = "Отсутствуют точки маршрута" });
                    }

                    _logger.LogMessage(User, 
                        $"Тестируется полёт по геоточкам - получено {request.Waypoints.Count} точек", 
                        LogLevel.Information);

                    // Логируем полученные точки
                    foreach (var (point, index) in request.Waypoints.Select((p, i) => (p, i)))
                    {
                        _logger.LogMessage(User, 
                            $"Точка {index + 1}: Lat={point.Latitude:F6}, Lon={point.Longitude:F6}", 
                            LogLevel.Debug);
                    }

                    // 1. Получаем текущую позицию дрона
                    var activeDroneUrl = DRONE_BASE_URL;
                    var dronePosition = await _missionPlanning.GetCurrentDronePosition(activeDroneUrl);
                    
                    if (dronePosition == null)
                    {
                        _logger.LogMessage(User, "Не удалось получить текущую позицию дрона", LogLevel.Error);
                        return Ok(new { status = "error", message = "Не удалось получить позицию дрона" });
                    }

                    var startPoint = dronePosition.Position;
                    
                    _logger.LogMessage(User, 
                        $"Текущая позиция дрона: Lat={startPoint.Latitude:F6}, Lon={startPoint.Longitude:F6}, Alt={startPoint.Altitude:F1}", 
                        LogLevel.Information);

                    // 2. Создаем миссию из всех точек (используем новый метод для массива точек)
                    // Параметр returnToHome = false - не возвращаемся в точку взлета, садимся в последней точке
                    var mission = await _missionPlanning.CreateFullQgcMission(
                        startPoint: startPoint,
                        waypoints: request.Waypoints,
                        returnToHome: false // Посадка в последней точке маршрута
                    );

                    var missionJson = JsonConvert.SerializeObject(mission, Formatting.Indented); _logger.LogMessage(User, 
                            $"СФОРМИРОВАНО ПОЛЁТНОЕ ЗАДАНИЕ (JSON):\n{missionJson}", LogLevel.Information);


                    if (mission == null)
                    {
                        _logger.LogMessage(User, "Не удалось создать миссию", LogLevel.Error);
                        return Ok(new { status = "error", message = "Не удалось создать миссию" });
                    }

                    // 4. Отправляем миссию на дрон
                    _logger.LogMessage(User, "Отправляем миссию на дрон...", LogLevel.Information);
                    
                    var result = await _droneConnection.SendCommandToDrone("execute-mission", mission);

                    if (!result.Success)
                    {
                        _logger.LogMessage(User, 
                            $"Не удалось отправить миссию на дрон: {result.ErrorMessage}", 
                            LogLevel.Error);
                        return Ok(new { 
                            status = "error", 
                            message = "Не удалось отправить миссию на дрон",
                            details = result.ErrorMessage
                        });
                    }

                    // 5. Логируем создание миссии
                    var lastWaypoint = request.Waypoints.Last();
                    await LogMissionCreation(request.Waypoints.Count, startPoint, lastWaypoint);

                    _logger.LogMessage(User, 
                        $"Миссия успешно отправлена на дрон! Точки: {request.Waypoints.Count}", 
                        LogLevel.Information);

                    return Ok(new { 
                        status = "success", 
                        message = "Миссия отправлена на дрон",
                        waypoints_count = request.Waypoints.Count,
                        start_point = new { 
                            latitude = startPoint.Latitude,
                            longitude = startPoint.Longitude,
                            altitude = startPoint.Altitude 
                        },
                        target_points = request.Waypoints.Select((w, i) => new { 
                            index = i + 1,
                            latitude = w.Latitude,
                            longitude = w.Longitude
                        }),
                        home_position_set = true // Домашняя позиция установлена автоматически
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                    return StatusCode(500, new { 
                        status = "error", 
                        message = ex.Message,
                        details = ex.StackTrace 
                    });
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
        public async Task<IActionResult> HomePosition(bool stat)
        {
            try
            {
                if (stat)
                {
                    _logger.LogMessage(User, 
                        "Получена команда returnHome = true (игнорируется, требуется false для возврата)", 
                        LogLevel.Information);
                    return Ok(new { 
                        status = "info", 
                        message = "Для возврата домой отправьте false"
                    });
                }

                const double hoverAltitude = 2; // Высота зависания над домашней позицией
                
                _logger.LogMessage(User, 
                    $"КОМАНДА: Возврат дрона в домашнюю позицию (зависание на {hoverAltitude}м)", 
                    LogLevel.Information);

                // 1. Получаем текущую позицию дрона
                var activeDroneUrl = DRONE_BASE_URL;
                var dronePosition = await _missionPlanning.GetCurrentDronePosition(activeDroneUrl);
                
                if (dronePosition == null)
                {
                    _logger.LogMessage(User, "Не удалось получить текущую позицию дрона", LogLevel.Error);
                    return Ok(new { status = "error", message = "Не удалось получить позицию дрона" });
                }

                var currentPos = dronePosition.Position;
                _logger.LogMessage(User, 
                    $"Текущая позиция дрона: Lat={currentPos.Latitude:F6}, Lon={currentPos.Longitude:F6}", 
                    LogLevel.Information);

                // 2. Получаем домашнюю позицию
                var homePosition = await _homePositionService.GetHomePosition();
                if (homePosition == null)
                {
                    _logger.LogMessage(User, 
                        "ОШИБКА: Домашняя позиция не установлена", 
                        LogLevel.Error);
                    return Ok(new { 
                        status = "error", 
                        message = "Домашняя позиция не установлена",
                        solution = "Сначала запустите миссию через /TestAutopilot"
                    });
                }

                _logger.LogMessage(User, 
                    $"Домашняя позиция: Lat={homePosition.Latitude:F6}, Lon={homePosition.Longitude:F6}", 
                    LogLevel.Information);

                // 3. Создаем миссию возврата домой
                var mission = await _missionPlanning.CreateReturnToHomeMission(
                    currentPosition: currentPos, 
                    altitude: hoverAltitude);
                
                // 4. Логируем полётное задание
                try
                {
                    var missionJson = JsonConvert.SerializeObject(mission, Formatting.Indented);
                    _logger.LogMessage(User, 
                        $"СФОРМИРОВАНО ПОЛЁТНОЕ ЗАДАНИЕ ВОЗВРАТА ДОМОЙ:\n{missionJson}", 
                        LogLevel.Information);
                }
                catch (Exception jsonEx)
                {
                    _logger.LogMessage(User, 
                        $"Не удалось сериализовать полётное задание: {jsonEx.Message}", 
                        LogLevel.Warning);
                }

                // 5. Отправляем миссию на дрон
                _logger.LogMessage(User, "Отправка команды возврата домой на дрон...", LogLevel.Information);
                
                var result = await _droneConnection.SendCommandToDrone("return-home-no-land", mission);
                
                if (!result.Success)
                {
                    _logger.LogMessage(User, 
                        $"ОШИБКА отправки команды дрону: {result.ErrorMessage}", 
                        LogLevel.Error);
                    return Ok(new { 
                        status = "error", 
                        message = "Не удалось отправить команду возврата домой",
                        drone_error = result.ErrorMessage
                    });
                }

                _logger.LogMessage(User, 
                    "УСПЕХ: Команда возврата домой отправлена на дрон", 
                    LogLevel.Information);

                return Ok(new { 
                    status = "success", 
                    message = "Дрон возвращается домой",
                    current_position = new { 
                        currentPos.Latitude, 
                        currentPos.Longitude 
                    },
                    home_position = new { 
                        homePosition.Latitude, 
                        homePosition.Longitude 
                    },
                    hover_altitude = hoverAltitude,
                    note = "Дрон зависнет над домашней позицией без посадки"
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, Auxiliary.GetDetailedExceptionMessage(ex), LogLevel.Error);
                return StatusCode(500, new { 
                    status = "error", 
                    message = ex.Message
                });
            }
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
                var message = logData.TryGetValue("MESSAGE", out object value) ? value.ToString() : "No message";
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


        /// <summary>
        /// Создает миссию из массива точек
        /// </summary>
        private async Task<object> CreateMissionFromWaypoints(GeoPoint startPoint, List<GeoPoint> waypoints)
        {
            try
            {
                // Создаем список всех точек маршрута
                var allPoints = new List<GeoPoint> { startPoint };
                allPoints.AddRange(waypoints);

                // Формируем миссию для дрона
                var mission = new
                {
                    takeoff_altitude = 10.0f, // Высота взлета
                    waypoints = allPoints.Select((point, index) => new
                    {
                        sequence = index,
                        latitude = point.Latitude,
                        longitude = point.Longitude,
                        altitude = point.Altitude > 0 ? point.Altitude : 10.0f, // Если высота не указана, используем 10м
                        speed = 5.0f // Скорость полета между точками
                    }).ToList(),
                    landing_at_end = true // Автоматическая посадка после завершения
                };

                _logger.LogMessage(User, 
                    $"Создана миссия с {allPoints.Count} точками (включая стартовую)", 
                    LogLevel.Information);

                return mission;
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка создания миссии: {ex.Message}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Логирование создания миссии
        /// </summary>
        private async Task LogMissionCreation(int waypointsCount, GeoPoint startPoint, GeoPoint lastPoint)
        {
            try
            {
                var logEntry = new
                {
                    Timestamp = DateTime.UtcNow,
                    Event = "mission_created",
                    User = User?.Identity?.Name ?? "anonymous",
                    WaypointsCount = waypointsCount,
                    StartPoint = new { startPoint.Latitude, startPoint.Longitude },
                    LastPoint = new { lastPoint.Latitude, lastPoint.Longitude },
                    Message = $"Создана миссия с {waypointsCount} точками от ({startPoint.Latitude}, {startPoint.Longitude}) до ({lastPoint.Latitude}, {lastPoint.Longitude})"
                };

                // Здесь можно добавить сохранение в базу данных или отправку в систему логирования
                _logger.LogMessage(User, logEntry.Message, LogLevel.Information);
            }
            catch (Exception ex)
            {
                _logger.LogMessage(User, $"Ошибка логирования миссии: {ex.Message}", LogLevel.Error);
            }
        }

    }
}