using System;

namespace Colibri.WebApi.DTO;

public class DroneTelemetryDto
{
    // Позиция
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public double RelativeAltitude { get; set; }

    // Батарея
    public double BatteryVoltage { get; set; }
    public double BatteryPercentage { get; set; }

    // Датчики
    public bool Gyro { get; set; }
    public bool Accel { get; set; }
    public bool Mag { get; set; }

    // GPS
    public string GpsStatus { get; set; }
    public int Satellites { get; set; }

    // Время
    public DateTime Timestamp { get; set; }

    // Дополнительные поля (если приходят)
    public double? Roll { get; set; }
    public double? Pitch { get; set; }
    public double? Yaw { get; set; }
    public double? GroundSpeed { get; set; }
    public bool? IsArmed { get; set; }
    public bool? IsInAir { get; set; }
}
