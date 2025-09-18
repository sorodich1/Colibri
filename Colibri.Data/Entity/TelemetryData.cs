using System;

namespace Colibri.Data.Entity;

public class TelemetryData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double RelativeAltitude { get; set; }
    public double BatteryAltitude { get; set; }
    public double BatteryPercentage { get; set; }
    public long Timestamp { get; set; }
}
