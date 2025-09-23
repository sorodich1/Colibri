using System;

namespace Colibri.WebApi.Models;

/// <summary>
/// Статус калибровки датчиков
/// </summary>
public class CalibrationStatus
{
    /// <summary>
    /// калибровка гироскопа
    /// </summary>
    public bool Gyro { get; set; }
    /// <summary>
    /// калибровка аксклерометра
    /// </summary>
    public bool Accelerometer { get; set; }
    /// <summary>
    /// калибровка магнитометра
    /// </summary>
    public bool Magnetometer { get; set; }
}
