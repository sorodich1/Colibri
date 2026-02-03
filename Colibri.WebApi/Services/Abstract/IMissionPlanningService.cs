using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.WebApi.Models;

namespace Colibri.WebApi.Services.Abstract;

public interface IMissionPlanningService
{
    Task<object> CreateFullQgcMission(GeoPoint startPoint, List<GeoPoint> waypoints, 
    double cruiseSpeed = 15, double altitude = 5, bool returnToHome = false, 
    double takeoffAltitude = 2, double hoverSpeed = 5);
    Task<DronePosition> GetCurrentDronePosition(string droneUrl);
    Task<object> CreateReturnToHomeMission(GeoPoint currentPosition, double altitude = 5);
}
