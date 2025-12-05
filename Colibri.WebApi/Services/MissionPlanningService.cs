using System;
using System.Collections.Generic;
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
    private readonly ILogger<MissionPlanningService> _logger;

    public MissionPlanningService(
        IDroneConnectionService droneConnection, 
        IHttpConnectService httpConnect,
        ILogger<MissionPlanningService> logger)
    {
        _droneConnection = droneConnection;
        _httpConnect = httpConnect;
        _logger = logger;
    }

    public async Task<object> CreateDeliveryMission(GeoPoint startPoint, GeoPoint destination, double cruiseSpeed = 15, double altitude = 5)
    {
        var mission = new Dictionary<string, object>
        {
            ["mission"] = new Dictionary<string, object>
            {
                ["plannedHomePosition"] = new double[] { startPoint.Latitude, startPoint.Longitude, altitude },
                ["items"] = new List<Dictionary<string, object>>
                {
                    new() {
                        ["command"] = 22, // TAKEOFF
                        ["params"] = new object[] { 0, 0, 0, 0, startPoint.Latitude, startPoint.Longitude, altitude }
                    },
                    new() {
                        ["command"] = 16, // WAYPOINT
                        ["params"] = new object[] { 0, 0, 0, 0, destination.Latitude, destination.Longitude, altitude }
                    },
                    new() {
                        ["command"] = 21, // LAND
                        ["params"] = new object[] { 0, 0, 0, 0, destination.Latitude, destination.Longitude, 0 }
                    }
                }
            }
        };

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