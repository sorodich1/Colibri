using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;

namespace Colibri.WebApi.Services;

public class MissionPlanningService(IDroneConnectionService droneConnection) : IMissionPlanningService
{
    private readonly IDroneConnectionService _droneConnection = droneConnection;

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
        // Создаем mission items с использованием dynamic
        var missionItems = new List<dynamic>
        {
            new
            {
                AMSLAltAboveTerrain = (object)null,
                Altitude = altitude,
                AltitudeMode = 1,
                autoContinue = true,
                command = 22, // MAV_CMD_NAV_TAKEOFF
                doJumpId = 1,
                frame = 3,
                parameters = new object[] { 0, 0, 0, null, startPoint.Latitude, startPoint.Longitude, altitude },
                type = "SimpleItem"
            },
            new
            {
                AMSLAltAboveTerrain = (object)null,
                Altitude = altitude,
                AltitudeMode = 1,
                autoContinue = true,
                command = 16, // MAV_CMD_NAV_WAYPOINT
                doJumpId = 2,
                frame = 3,
                parameters = new object[] { 0, 0, 0, null, destination.Latitude, destination.Longitude, altitude },
                type = "SimpleItem"
            },
            new
            {
                AMSLAltAboveTerrain = (object)null,
                Altitude = 0,
                AltitudeMode = 1,
                autoContinue = true,
                command = 21, // MAV_CMD_NAV_LAND
                doJumpId = 3,
                frame = 3,
                parameters = new object[] { 0, 0, 0, null, destination.Latitude, destination.Longitude, 0 },
                type = "SimpleItem"
            }
        };

        var missionTemplate = new
        {
            fileType = "Plan",
            geoFence = new
            {
                circles = new object[0],
                polygons = new object[0],
                version = 2
            },
            groundStation = "QGroundControl",
            mission = new
            {
                cruiseSpeed = cruiseSpeed,
                firmwareType = 3,
                globalPlanAltitudeMode = 1,
                hoverSpeed = 5,
                items = missionItems, // используем List<dynamic>
                plannedHomePosition = new[] { startPoint.Latitude, startPoint.Longitude, altitude },
                vehicleType = 2,
                version = 2
            },
            rallyPoints = new
            {
                points = new object[0],
                version = 2
            },
            version = 1
        };

        return await Task.FromResult(missionTemplate);
    }

    public async Task<DronePosition> GetCurrentDronePosition(string droneUrl)
    {
        try
        {
            // ЗАГЛУШКА: статические координаты дрона для тестирования
            return new DronePosition
            {
                Position = new GeoPoint
                {
                    Latitude = 55.7558,  // Статические координаты для теста
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
            throw new Exception($"Ошибка получения позиции дрона: {ex.Message}");
        }
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
