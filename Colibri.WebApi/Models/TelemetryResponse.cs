using System;

namespace Colibri.WebApi.Models;

/// <summary>
/// 
/// </summary>
public class TelemetryResponse
{
    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
