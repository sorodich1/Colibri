using System;

namespace Colibri.WebApi.Models;

public class DronePosition
{
    public GeoPoint Position { get; set; }
    public double Speed { get; set; }
    public double Course { get; set; }
    public int Satellites { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; }
}
