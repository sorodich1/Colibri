using Colibri.Data.Helpers;

namespace Colibri.Data.Entity;

/// <summary>
/// Сущность телеметрии дрона
/// </summary>
public class Telemetry : AudiTable
{
    /// <summary>
    /// Широта
    /// </summary>
    public decimal Latitude {get; set; }
    /// <summary>
    /// Долгота
    /// </summary>
    public decimal Longitude {get; set; }
    /// <summary>
    /// Высота
    /// </summary>
    public decimal Altitude {get; set; }
    /// <summary>
    /// Относительная высота
    /// </summary>
    public decimal RelativeAltitude {get; set; }
    /// <summary>
    /// Напряжение батареи
    /// </summary>
    public decimal BatteryVoltage {get; set; }
    /// <summary>
    /// Процент заряда батареи
    /// </summary>
    public decimal BatteryPercentage {get; set; }
    /// <summary>
    /// Колибровка гироскопа
    /// </summary>
    public bool Gyro { get; set; }
    /// <summary>
    /// Колибровка акселерометра
    /// </summary>
    public bool Accel { get; set; }
    /// <summary>
    /// Калибровка магнитометра
    /// </summary>
    public bool Mag { get; set; }
    /// <summary>
    /// Состояние gps датчиков
    /// </summary>
    public string GpsStatus { get; set; }
}
