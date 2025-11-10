using System.Collections.Generic;
using System.Threading.Tasks;
using Colibri.WebApi.Models;

namespace Colibri.WebApi.Services.Abstract;

public interface IDroneConnectionService
{
        Task<DroneConnectionResult> SendCommandToDrone(string endpoint, object command);
        Task<bool> TestDroneConnection(string droneUrl);
        Task<string> GetActiveDroneUrl();
        Task SwitchToNextDrone();
        List<DroneConfig> GetAvailableDrones();
}
