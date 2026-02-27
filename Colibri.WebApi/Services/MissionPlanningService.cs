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

        var missionItems = new List<Dictionary<string, object>>();
        int doJumpId = 1;

        // 1. ВЗЛЁТ
        missionItems.Add(new Dictionary<string, object>
        {
            ["AMSLAltAboveTerrain"] = null,
            ["Altitude"] = takeoffAltitude,
            ["AltitudeMode"] = 1,
            ["autoContinue"] = true,
            ["command"] = 22,
            ["doJumpId"] = doJumpId++,
            ["frame"] = 3,
            ["params"] = new object[] { 0, 0, 0, 0, startPoint.Latitude, startPoint.Longitude, takeoffAltitude },
            ["type"] = "SimpleItem"
        });

        // 2. ПУТЕВЫЕ ТОЧКИ (только 16 команды, без 178 между ними)
        for (int i = 0; i < waypoints.Count; i++)
        {
            var waypoint = waypoints[i];
            double pointAltitude = waypoint.Altitude > 0 ? waypoint.Altitude : altitude;
            
            missionItems.Add(new Dictionary<string, object>
            {
                ["AMSLAltAboveTerrain"] = null,
                ["Altitude"] = pointAltitude,
                ["AltitudeMode"] = 1,
                ["autoContinue"] = true,
                ["command"] = 16,
                ["doJumpId"] = doJumpId++,
                ["frame"] = 3,
                ["params"] = new object[] { 0, 0, 0, 0, waypoint.Latitude, waypoint.Longitude, pointAltitude },
                ["type"] = "SimpleItem"
            });
        }

        // 3. ПОСАДКА В ПОСЛЕДНЕЙ ТОЧКЕ
        var landingPoint = waypoints.Last();
        missionItems.Add(new Dictionary<string, object>
        {
            ["AMSLAltAboveTerrain"] = null,
            ["Altitude"] = 0,
            ["AltitudeMode"] = 1,
            ["autoContinue"] = true,
            ["command"] = 21, // LAND - гарантированная посадка
            ["doJumpId"] = doJumpId++,
            ["frame"] = 3,
            ["params"] = new object[] { 0, 0, 0, 0, landingPoint.Latitude, landingPoint.Longitude, 0 },
            ["type"] = "SimpleItem"
        });

        // Формируем миссию
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
                ["firmwareType"] = 3,
                ["globalPlanAltitudeMode"] = 0,
                ["hoverSpeed"] = hoverSpeed,
                ["items"] = missionItems,
                ["plannedHomePosition"] = new double[] { startPoint.Latitude, startPoint.Longitude, takeoffAltitude },
                ["vehicleType"] = 2,
                ["version"] = 2
            },
            ["rallyPoints"] = new Dictionary<string, object>
            {
                ["points"] = new List<object>(),
                ["version"] = 2
            },
            ["version"] = 1
        };

        _logger.LogInformation($"✅ Миссия создана: {waypoints.Count} точек, посадка в последней точке");
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