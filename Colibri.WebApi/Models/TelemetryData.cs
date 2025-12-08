using System.Text.Json.Serialization;

namespace Colibri.WebApi.Models;

/// <summary>
/// 
/// </summary>
public class TelemetryData
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("altitude")]
    public double Altitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("relativeAltitude")]
    public double RelativeAltitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("batteryVoltage")]
    public double BatteryVoltage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("batteryPercentage")]
    public double BatteryPercentage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("calibrationStatus")]
    public string CalibrationStatus { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("gpsStatus")]
    public string GpsStatus { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}
