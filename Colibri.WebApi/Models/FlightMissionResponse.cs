using System;

namespace Colibri.WebApi.Models;

public class FlightMissionResponse
{
    public bool Success { get; set; }
    public string MissionId { get; set; }
    public GeoPoint StartPoint { get; set; }
    public GeoPoint EndPoint { get; set; }
    public double TotalDistance { get; set; }
    public TimeSpan EstimatedTime { get; set; }
    public object MissionData { get; set; }
    public string DroneUrl { get; set; }
}
