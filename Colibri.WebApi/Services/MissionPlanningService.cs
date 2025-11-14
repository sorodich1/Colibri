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

    public double CalculateDistance(GeoPoint point1, GeoPoint point2)
    {
        // Расчет расстояния по формуле гаверсинусов
        var R = 6371; // Радиус Земли в км
        var dLat = ToRadians(point2.Latitude - point1.Latitude);
        var dLon = ToRadians(point2.Longitude - point1.Longitude);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(point1.Latitude)) * Math.Cos(ToRadians(point2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c; // Расстояние в км
        
        return distance * 1000; // Возвращаем в метрах
    }

    public TimeSpan CalculateEstimatedTime(double distance, double speed)
    {
        var timeHours = distance / 1000 / speed; // время в часах
        return TimeSpan.FromHours(timeHours);
    }

        public async Task<object> CreateDeliveryMission(GeoPoint startPoint, GeoPoint destination, double cruiseSpeed = 15, double altitude = 10)
        {
            var mission = new Dictionary<string, object>
            {
                ["mission"] = new Dictionary<string, object>
                {
                    ["plannedHomePosition"] = new double[] { startPoint.Latitude, startPoint.Longitude, altitude },
                    ["items"] = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            ["command"] = 22,
                            ["params"] = new object[] { 0, 0, 0, 0, startPoint.Latitude, startPoint.Longitude, altitude }
                        },
                        new Dictionary<string, object>
                        {
                            ["command"] = 16,
                            ["params"] = new object[] { 0, 0, 0, 0, destination.Latitude, destination.Longitude, altitude }
                        },
                        new Dictionary<string, object>
                        {
                            ["command"] = 21,
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
                        Status = "Connected (Live)"
                    };
                }
                else
                {
                    _logger.LogWarning($"Дрон вернул статус ошибки: {positionData.status}");
                }
            }
            
            // Если не удалось получить реальные координаты, используем заглушку
            _logger.LogWarning($"Не удалось получить реальные координаты с дрона {droneUrl}, использую заглушку");
            return new DronePosition
            {
                Position = new GeoPoint
                {
                    Latitude = 55.7558,
                    Longitude = 37.6173,
                    Altitude = 0
                },
                Speed = 0,
                Course = 0,
                Satellites = 12,
                Timestamp = DateTime.UtcNow,
                Status = "Connected (Simulation)"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка получения позиции дрона: {ex.Message}");
            
            // Возвращаем заглушку при ошибке
            return new DronePosition
            {
                Position = new GeoPoint
                {
                    Latitude = 55.7558,
                    Longitude = 37.6173,
                    Altitude = 0
                },
                Speed = 0,
                Course = 0,
                Satellites = 12,
                Timestamp = DateTime.UtcNow,
                Status = $"Error: {ex.Message}"
            };
        }
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}