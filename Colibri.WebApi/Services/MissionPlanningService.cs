using System;
using System.Threading.Tasks;
using Colibri.WebApi.Models;
using Colibri.WebApi.Services.Abstract;

namespace Colibri.WebApi.Services;

public class MissionPlanningService(IDroneConnectionService droneConnection) : IMissionPlanningService
{
    private readonly IDroneConnectionService _droneConnection = droneConnection;

    public double CalculateDistance(GeoPoint point1, GeoPoint point2)
    {
        throw new NotImplementedException();
    }

    public TimeSpan CalculateEstimatedTime(double distance, double speed)
    {
        throw new NotImplementedException();
    }

    public Task<object> CreateDeliveryMission(GeoPoint startPoint, GeoPoint destination, double cruiseSpeed = 15, double altitude = 10)
    {
        // Создаем явно типизированные объекты для элементов миссии
        var takeoffItem = new
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
        };

        var waypointItem = new
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
        };

        var landItem = new
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
        };

        // Создаем массив с явно указанными объектами
        var missionItems = new[] { takeoffItem, waypointItem, landItem };

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
                items = missionItems,
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

        return Task.FromResult<object>(missionTemplate);
    }

    public async Task<DronePosition> GetCurrentDronePosition(string droneUrl)
    {
        try
        {
            var result = await _droneConnection.SendCommandToDrone("api/telemetry/position", new { Command = "GET_POSITION" });

            if (result.Success)
            {
                return new DronePosition
                {
                    Position = new GeoPoint
                    {
                        Latitude = 59.85607073,
                        Longitude = 30.26375508,
                        Altitude = 10
                    },
                    Timestamp = DateTime.UtcNow,
                    Status = "Connected"
                };
            }
            throw new Exception("Не удалось получить позицию дрона");
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка получения позиции дрона: {ex.Message}");
        }
    }
}
