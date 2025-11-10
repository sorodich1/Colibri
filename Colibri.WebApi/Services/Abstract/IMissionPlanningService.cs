using System;
using System.Threading.Tasks;
using Colibri.WebApi.Models;

namespace Colibri.WebApi.Services.Abstract;

public interface IMissionPlanningService
{
    Task<object> CreateDeliveryMission(GeoPoint startPoint, GeoPoint destination, double cruiseSpeed = 15, double altitude = 10);
    Task<DronePosition> GetCurrentDronePosition(string droneUrl);
    double CalculateDistance(GeoPoint point1, GeoPoint point2);
    TimeSpan CalculateEstimatedTime(double distance, double speed);
}
