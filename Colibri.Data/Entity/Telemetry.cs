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
    public double Latitude {get; set; }
    /// <summary>
    /// Долгота
    /// </summary>
    public double Longitude {get; set; }
    /// <summary>
    /// Высота
    /// </summary>
    public double Altitude {get; set; }
    /// <summary>
    /// Относительная высота
    /// </summary>
    public double RelativeAltitude {get; set; }
    /// <summary>
    /// Напряжение батареи
    /// </summary>
    public double BatteryVoltage {get; set; }
    /// <summary>
    /// Процент заряда батареи
    /// </summary>
    public double BatteryPercentage {get; set; }
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
