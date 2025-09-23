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
    public decimal Latitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("longitude")]
    public decimal Longitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("altitude")]
    public decimal Altitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("relativeAltitude")]
    public decimal RelativeAltitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("batteryVoltage")]
    public decimal BatteryVoltage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("batteryPercentage")]
    public decimal BatteryPercentage { get; set; }

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
