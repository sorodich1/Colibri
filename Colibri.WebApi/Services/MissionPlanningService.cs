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

    public async Task<object> CreateFullQgcMission(GeoPoint startPoint, List<GeoPoint> waypoints, 
    double cruiseSpeed = 15, double altitude = 5, bool returnToHome = false, 
    double takeoffAltitude = 2, double hoverSpeed = 5)
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            throw new ArgumentException("Массив точек маршрута не может быть пустым");
        }

        // ВСЕГДА обновляем домашнюю позицию при создании новой миссии
        var savedHomePosition = await _homePositionService.GetHomePosition();
        
        if (savedHomePosition ==null )
        {
           _logger.LogWarning("Домашняя позиция ещё не установлена. Рекомендуется сначала вызвать SetConfirmGeolocation");
        }
        else
        {
            _logger.LogDebug($"Используем сохранённую домашнюю позицию: Lat={savedHomePosition.Latitude:F6}, Lon={savedHomePosition.Longitude:F6}");
        }

        var missionItems = new List<Dictionary<string, object>>();
        int doJumpId = 1;

        // 1. Первая команда изменения скорости (в начале миссии)
        missionItems.Add(new Dictionary<string, object>
        {
            ["autoContinue"] = true,
            ["command"] = 178, // MAV_CMD_DO_CHANGE_SPEED
            ["doJumpId"] = doJumpId++,
            ["frame"] = 2, // MAV_FRAME_MISSION
            ["params"] = new object[] { 1, hoverSpeed, -1, 0, 0, 0, 0 },
            ["type"] = "SimpleItem"
        });

        // 2. Команда взлета (на повышенную высоту как в QGC)
        missionItems.Add(new Dictionary<string, object>
        {
            ["AMSLAltAboveTerrain"] = null,
            ["Altitude"] = takeoffAltitude,
            ["AltitudeMode"] = 1, // Relative to home
            ["autoContinue"] = true,
            ["command"] = 22, // MAV_CMD_NAV_TAKEOFF
            ["doJumpId"] = doJumpId++,
            ["frame"] = 3, // MAV_FRAME_GLOBAL_RELATIVE_ALT
            ["params"] = new object[] { 0, 0, 0, null, startPoint.Latitude, startPoint.Longitude, takeoffAltitude },
            ["type"] = "SimpleItem"
        });

        // 3. Команды для каждой точки маршрута с командами скорости между ними
        for (int i = 0; i < waypoints.Count; i++)
        {
            var waypoint = waypoints[i];
            
            // Используем высоту из waypoint, если она задана, иначе высоту по умолчанию
            double pointAltitude = waypoint.Altitude > 0 ? waypoint.Altitude : altitude;
            
            // Команда точки маршрута
            missionItems.Add(new Dictionary<string, object>
            {
                ["AMSLAltAboveTerrain"] = null,
                ["Altitude"] = pointAltitude,
                ["AltitudeMode"] = 1,
                ["autoContinue"] = true,
                ["command"] = 16, // MAV_CMD_NAV_WAYPOINT
                ["doJumpId"] = doJumpId++,
                ["frame"] = 3,
                ["params"] = new object[] { 0, 0, 0, 0, waypoint.Latitude, waypoint.Longitude, pointAltitude },
                ["type"] = "SimpleItem"
            });

            _logger.LogDebug($"Добавлена точка маршрута {i+1}: Lat={waypoint.Latitude:F6}, Lon={waypoint.Longitude:F6}, Alt={pointAltitude}м");

            // Если это не последняя точка - добавляем команду скорости
            if (i < waypoints.Count - 1)
            {
                missionItems.Add(new Dictionary<string, object>
                {
                    ["autoContinue"] = true,
                    ["command"] = 178,
                    ["doJumpId"] = doJumpId++,
                    ["frame"] = 2,
                    ["params"] = new object[] { 1, hoverSpeed, -1, 0, 0, 0, 0 },
                    ["type"] = "SimpleItem"
                });
            }
        }

        // 4. Если нужно вернуться в точку взлета (домашнюю позицию)
        if (returnToHome)
        {
            var homePosition = await _homePositionService.GetHomePosition();
            if (homePosition == null)
            {
                _logger.LogWarning("Запрошен возврат домой, но домашняя позиция не установлена. Используем startPoint как запасной вариант.");
            }
            else
            {
                _logger.LogInformation($"Возврат в сохранённую домашнюю позицию: Lat={homePosition.Latitude:F6}, Lon={homePosition.Longitude:F6}");
            }

            // Команда скорости перед возвратом домой
            missionItems.Add(new Dictionary<string, object>
            {
                ["autoContinue"] = true,
                ["command"] = 178,
                ["doJumpId"] = doJumpId++,
                ["frame"] = 2,
                ["params"] = new object[] { 1, hoverSpeed, -1, 0, 0, 0, 0 },
                ["type"] = "SimpleItem"
            });

            // Команда точки возврата домой (на высоте последней точки или altitude)
            double returnAltitude = waypoints.Count > 0 ? waypoints.Last().Altitude : altitude;
            if (returnAltitude <= 0) returnAltitude = altitude;
            
            missionItems.Add(new Dictionary<string, object>
            {
                ["AMSLAltAboveTerrain"] = null,
                ["Altitude"] = returnAltitude,
                ["AltitudeMode"] = 1,
                ["autoContinue"] = true,
                ["command"] = 16, // WAYPOINT к домашней позиции
                ["doJumpId"] = doJumpId++,
                ["frame"] = 3,
                ["params"] = new object[] { 0, 0, 0, 0, startPoint.Latitude, startPoint.Longitude, returnAltitude },
                ["type"] = "SimpleItem"
            });
        }

        // 5. Команда скорости перед посадкой
        missionItems.Add(new Dictionary<string, object>
        {
            ["autoContinue"] = true,
            ["command"] = 178,
            ["doJumpId"] = doJumpId++,
            ["frame"] = 2,
            ["params"] = new object[] { 1, 2, -1, 0, 0, 0, 0 }, // Скорость посадки 2 м/с
            ["type"] = "SimpleItem"
        });

        // 6. Команда посадки (в последней точке или дома)
        GeoPoint landingPoint;

        if (returnToHome)
        {
            // При возврате домой садимся в сохранённую домашнюю позицию
            var homePosition = await _homePositionService.GetHomePosition();
            landingPoint = homePosition ?? startPoint;
            _logger.LogInformation($"Посадка в домашней позиции: ({landingPoint.Latitude:F6}, {landingPoint.Longitude:F6})");
        }
        else
        {
            // Иначе садимся в последней точке маршрута
            landingPoint = waypoints.Last();
            _logger.LogInformation($"Посадка в последней точке маршрута: ({landingPoint.Latitude:F6}, {landingPoint.Longitude:F6})");
        }

        missionItems.Add(new Dictionary<string, object>
        {
            ["AMSLAltAboveTerrain"] = null,
            ["Altitude"] = 0,
            ["AltitudeMode"] = 1,
            ["autoContinue"] = true,
            ["command"] = 21, // MAV_CMD_NAV_LAND
            ["doJumpId"] = doJumpId++,
            ["frame"] = 3,
            ["params"] = new object[] { 0, 0, 0, null, landingPoint.Latitude, landingPoint.Longitude, 0 },
            ["type"] = "SimpleItem"
        });
        var displayHomePosition = await _homePositionService.GetHomePosition() ?? startPoint;
        // Формируем полную миссию в формате QGC
        var mission = new Dictionary<string, object>
        {
            ["fileType"] = "Plan",
            ["geoFence"] = new Dictionary<string, object>
            {
                ["circles"] = new List<object>(),
                ["polygons"] = new List<object>(),
                ["version"] = 2
            },
            ["groundStation"] = "QGroundControl",
            ["mission"] = new Dictionary<string, object>
            {
                ["cruiseSpeed"] = cruiseSpeed,
                ["firmwareType"] = 3, // ArduPilot
                ["globalPlanAltitudeMode"] = 0, // Relative
                ["hoverSpeed"] = hoverSpeed,
                ["items"] = missionItems,
                ["plannedHomePosition"] = new double[] { displayHomePosition.Latitude, displayHomePosition.Longitude, takeoffAltitude },
                ["vehicleType"] = 2, // Quadrotor
                ["version"] = 2
            },
            ["rallyPoints"] = new Dictionary<string, object>
            {
                ["points"] = new List<object>(),
                ["version"] = 2
            },
            ["version"] = 1
        };

        _logger.LogInformation($"Создана ПОЛНАЯ QGC миссия с {waypoints.Count} точками маршрута. " +
                            $"Возврат домой: {returnToHome}, Высота взлета: {takeoffAltitude}м, " +
                            $"Крейсерская скорость: {cruiseSpeed}м/с, Скорость зависания: {hoverSpeed}м/с, " +
                            $"Всего команд в миссии: {missionItems.Count}");

        // Логируем детали для отладки
        _logger.LogDebug($"Структура миссии:");
        _logger.LogDebug($"1. Команда скорости (178) - начало");
        _logger.LogDebug($"2. Взлет (22) на {takeoffAltitude}м");
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            _logger.LogDebug($"{i + 3}. Точка {i + 1} (16) - {waypoints[i].Latitude:F6}, {waypoints[i].Longitude:F6}");
            if (i < waypoints.Count - 1)
            {
                _logger.LogDebug($"{i + 4}. Команда скорости (178)");
            }
        }
        
        _logger.LogDebug($"{waypoints.Count + 3}. Команда посадки (21) - {landingPoint.Latitude:F6}, {landingPoint.Longitude:F6}");

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

    public Task<object> CreateReturnToHomeMission(GeoPoint currentPosition, double altitude = 5)
    {
        throw new NotImplementedException();
    }





    // public async Task<DronePosition> GetCurrentDronePosition(string droneUrl)
    // {
    //     try
    //     {
    //         _logger.LogInformation($"Получаем позицию дрона с URL: {droneUrl}");

    //         // ВРЕМЕННО: используем фиктивные координаты
    //         var fakeLatitude = 59.886053; 
    //         var fakeLongitude = 30.485970;
    //         var fakeAltitude = 5.0;

    //         _logger.LogInformation($"Используются фиктивные координаты: {fakeLatitude}, {fakeLongitude}, высота: {fakeAltitude}м");

    //         return new DronePosition
    //         {
    //             Position = new GeoPoint
    //             {
    //                 Latitude = fakeLatitude,
    //                 Longitude = fakeLongitude,
    //                 Altitude = fakeAltitude
    //             },
    //             Speed = 0,
    //             Course = 0,
    //             Satellites = 12,
    //             Timestamp = DateTime.UtcNow,
    //             Status = "Connected (Test Data)"
    //         };
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError($"Ошибка получения позиции дрона: {ex.Message}");

    //         // Возвращаем фиктивные координаты даже при ошибке
    //         return new DronePosition
    //         {
    //             Position = new GeoPoint
    //             {
    //                 Latitude = 59.934280,
    //                 Longitude = 30.335098,
    //                 Altitude = 5.0
    //             },
    //             Speed = 0,
    //             Course = 0,
    //             Satellites = 12,
    //             Timestamp = DateTime.UtcNow,
    //             Status = $"Error: {ex.Message}"
    //         };
    //     }
    //}
}