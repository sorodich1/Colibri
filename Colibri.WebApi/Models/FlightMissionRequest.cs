using System;

namespace Colibri.WebApi.Models;

public class FlightMissionRequest
{
    public GeoPoint Destination { get; set; }
    public double? CruiseSpeed { get; set; } = 15;
    public double? HoverSpeed { get; set; } = 5;
    public double? Altitude { get; set; } = 10;
    public string MissionType { get; set; } = "DELIVERY"; // DELIVERY, SURVEY, etc.
}
