using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colibri.ConnectNetwork.Services.Abstract;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Colibri.WebApi.Services;

public class MissionPlanningService : IMissionPlanningService
{
    private readonly IDroneConnectionService _droneConnection;
    private readonly IHttpConnectService _httpConnect;
    private readonly IHomePositionService _homePositionService;
    private readonly ILogger<MissionPlanningService> _logger;
    private static GeoPoint _homePosition = null;
    private static bool _homePositionSet = false;

    public MissionPlanningService(
        IDroneConnectionService droneConnection, 
        IHttpConnectService httpConnect,
        IHomePositionService homePositionService,
        ILogger<MissionPlanningService> logger)
    {
        _droneConnection = droneConnection;
        _httpConnect = httpConnect;
        _logger = logger;
        _homePositionService = homePositionService;
    }

    public async Task<object> CreateDeliveryMission(GeoPoint startPoint, List<GeoPoint> waypoints, 
    double cruiseSpeed = 15, double altitude = 5, bool returnToHome = false)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            throw new ArgumentException("Массив точек маршрута не может быть пустым");
        }

        // ВСЕГДА обновляем домашнюю позицию при создании новой миссии
        var homeSetResult = await _homePositionService.SetHomePosition(startPoint);
        
        if (!homeSetResult)
        {
            _logger.LogWarning($"Не удалось обновить домашнюю позицию для новой миссии");
        }
        else
        {
            _logger.LogInformation($"Домашняя позиция ОБНОВЛЕНА для миссии с {waypoints.Count} точками: " +
                                $"Lat={startPoint.Latitude:F6}, Lon={startPoint.Longitude:F6}");
        }

        // Создаем список команд для миссии
        var missionItems = new List<Dictionary<string, object>>
        {
            // 1. Команда взлета
            new() {
                ["command"] = 22, // TAKEOFF
                ["params"] = new object[] { 0, 0, 0, 0, startPoint.Latitude, startPoint.Longitude, altitude }
            }
        };

        // 2. Команды для каждой точки маршрута
        foreach (var waypoint in waypoints)
        {
            missionItems.Add(new Dictionary<string, object>
            {
                ["command"] = 16, // WAYPOINT
                ["params"] = new object[] { 0, 0, 0, 0, waypoint.Latitude, waypoint.Longitude, altitude }
            });
            
            _logger.LogDebug($"Добавлена точка маршрута: Lat={waypoint.Latitude:F6}, Lon={waypoint.Longitude:F6}");
        }

        // 3. Если нужно вернуться в точку взлета (домашнюю позицию)
        if (returnToHome)
        {
            missionItems.Add(new Dictionary<string, object>
            {
                ["command"] = 16, // WAYPOINT к домашней позиции
                ["params"] = new object[] { 0, 0, 0, 0, startPoint.Latitude, startPoint.Longitude, altitude }
            });
        }

        // 4. Команда посадки (в последней точке или дома)
        var landingPoint = returnToHome ? startPoint : waypoints.Last();
        missionItems.Add(new Dictionary<string, object>
        {
            ["command"] = 21, // LAND
            ["params"] = new object[] { 0, 0, 0, 0, landingPoint.Latitude, landingPoint.Longitude, 0 }
        });

        // Формируем полную миссию
        var mission = new Dictionary<string, object>
        {
            ["mission"] = new Dictionary<string, object>
            {
                ["plannedHomePosition"] = new double[] { startPoint.Latitude, startPoint.Longitude, altitude },
                ["items"] = missionItems
            }
        };

        _logger.LogInformation($"Создана миссия с {waypoints.Count} точками маршрута. " +
                            $"Возврат домой: {returnToHome}, Высота: {altitude}м");

        return await Task.FromResult(mission);
    }

    // Добавляем новый метод для создания миссии возврата домой
        public async Task<object> CreateReturnToHomeMission(GeoPoint currentPosition, double altitude = 2)
        {
            // Получаем домашнюю позицию из отдельного сервиса
            var homePosition = await _homePositionService.GetHomePosition();
            
            if (homePosition == null)
            {
                _logger.LogError("Попытка создать миссию возврата домой, но домашняя позиция не установлена");
                throw new InvalidOperationException("Домашняя позиция не установлена. Сначала запустите обычную миссию.");
            }
            
            _logger.LogInformation($"Создаем миссию возврата домой. Текущая позиция: Lat={currentPosition.Latitude:F6}, Lon={currentPosition.Longitude:F6}");
            _logger.LogInformation($"Целевая домашняя позиция: Lat={homePosition.Latitude:F6}, Lon={homePosition.Longitude:F6}, Высота: {altitude}м");
            
            var mission = new Dictionary<string, object>
            {
                ["mission"] = new Dictionary<string, object>
                {
                    ["plannedHomePosition"] = new double[] { homePosition.Latitude, homePosition.Longitude, altitude },
                    ["items"] = new List<Dictionary<string, object>>
                    {
                        new() {
                            ["command"] = 22, // TAKEOFF (если дрон уже в воздухе, эта команда будет проигнорирована)
                            ["params"] = new object[] { 0, 0, 0, 0, currentPosition.Latitude, currentPosition.Longitude, altitude }
                        },
                        new() {
                            ["command"] = 16, // WAYPOINT к домашней позиции (на заданной высоте)
                            ["params"] = new object[] { 0, 0, 0, 0, homePosition.Latitude, homePosition.Longitude, altitude }
                        },
                        // УБИРАЕМ команду LAND - дрон будет висеть на месте
                        // Вместо посадки добавляем команду зависания (LOITER)
                        new() {
                            ["command"] = 17, // LOITER - зависание на месте
                            ["params"] = new object[] { 0, 0, 0, 0, 0, 0, 0 } // Параметры зависания
                        }
                        // Примечание: Если нужно зависать определенное время, можно добавить команду LOITER_TIME
                        // или просто оставить дрон висеть на последней точке
                    }
                }
            };
            
            _logger.LogInformation($"Миссия возврата домой создана. Дрон зависнет на высоте {altitude}м над домашней позицией");
            
            return await Task.FromResult(mission);
        }

    public async Task<DronePosition> GetCurrentDronePosition(string droneUrl)
    {
        try
        {
            _logger.LogInformation($"Получаем позицию дрона с URL: {droneUrl}");

            // Получаем реальные координаты с дрона через endpoint /get-position
            var response = await _httpConnect.GetAsync($"{droneUrl}/get-position");
            
            if (!string.IsNullOrEmpty(response))
            {
                var positionData = JsonConvert.DeserializeObject<dynamic>(response);
                
                // Проверяем статус ответа
                if (positionData.status == "success")
                {
                    _logger.LogInformation($"Получены реальные координаты: {positionData.latitude}, {positionData.longitude}");
                    
                    return new DronePosition
                    {
                        Position = new GeoPoint
                        {
                            Latitude = (double)positionData.latitude,
                            Longitude = (double)positionData.longitude,
                            Altitude = (double)positionData.altitude
                        },
                        Speed = 0,
                        Course = 0,
                        Satellites = positionData.satellites != null ? (int)positionData.satellites : 0,
                        Timestamp = DateTime.UtcNow,
                        Status = "Connected"
                    };
                }
            }
            
            // Если не удалось получить реальные координаты - возвращаем ошибку
            _logger.LogWarning($"Не удалось получить реальные координаты с дрона {droneUrl}");
            return new DronePosition
            {
                Position = new GeoPoint
                {
                    Latitude = 0,
                    Longitude = 0,
                    Altitude = 0
                },
                Speed = 0,
                Course = 0,
                Satellites = 0,
                Timestamp = DateTime.UtcNow,
                Status = "Ошибка получения координат"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка получения позиции дрона: {ex.Message}");
            
            // Возвращаем ошибку
            return new DronePosition
            {
                Position = new GeoPoint
                {
                    Latitude = 0,
                    Longitude = 0,
                    Altitude = 0
                },
                Speed = 0,
                Course = 0,
                Satellites = 0,
                Timestamp = DateTime.UtcNow,
                Status = $"Ошибка: {ex.Message}"
            };
        }
    }
}